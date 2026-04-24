using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Services
{
    /// <summary>
    /// Handles the OTP resend flow for email verification.
    ///
    /// Step-by-step (mirrors the API specification exactly):
    ///   1. Validate email format
    ///   2. JWT validation — handled upstream by [Authorize] + middleware
    ///   3. Compare request email with token email (cross-account guard)
    ///   4. Check user exists in DB; if not → 404
    ///   5. Check email not already verified; if yes → 409
    ///   6. First-time path — no existing OTP record → create new
    ///   7. Rate-limit check — if within window AND count ≥ max → 429
    ///      Otherwise increment count, update record, send OTP
    ///
    /// Security:
    ///   • Only the BCrypt hash of the raw OTP is persisted — plain-text is emailed only.
    ///   • Email comparison is normalised (trim + lower) to prevent case-bypass attacks.
    ///   • Rate limit window and max count are configurable via appsettings.json.
    /// </summary>
    public sealed class ResendOtpService : IResendOtpService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailOtpRepository _emailOtpRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResendOtpService> _logger;

        // ── Rate-limit defaults (overridable via configuration) ───────────────
        private readonly int _rateLimitWindowHours;   // x  — sliding window
        private readonly int _rateLimitMaxAttempts;   // y  — max sends per window

        // ── OTP settings ──────────────────────────────────────────────────────
        private const int OtpExpiryMinutes = 5;

        public ResendOtpService(
            IUserRepository userRepository,
            IEmailOtpRepository emailOtpRepository,
            IEmailService emailService,
            ILogger<ResendOtpService> logger,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _emailOtpRepository = emailOtpRepository;
            _emailService = emailService;
            _logger = logger;

            var section = configuration.GetSection("OtpRateLimit");
            _rateLimitWindowHours = int.TryParse(section["WindowHours"], out var w) ? w : 1;
            _rateLimitMaxAttempts = int.TryParse(section["MaxAttempts"], out var m) ? m : 3;
        }

        // ─────────────────────────────────────────────────────────────────────

        public async Task<ResendOtpResult> ResendOtpAsync(
    ResendOtpRequestDto request,
    CancellationToken ct = default)
        {
            // ── STEP 1: Validate email ───────────────────────────────
            if (string.IsNullOrWhiteSpace(request.Email) ||
                !ValidationHelper.IsValidEmail(request.Email))
            {
                return ResendOtpResult.Failure(
                    422,
                    "invalid request",
                    "INVALID_DATA",
                    "Email format is invalid or empty.");
            }

            var email = request.Email.Trim().ToLowerInvariant();

            // ── STEP 2: Check user exists ────────────────────────────
            var user = await _userRepository.GetByEmailAsync(email, ct);
            if (user is null)
            {
                return ResendOtpResult.Failure(
                    404,
                    "account not existed",
                    "USER_NOT_FOUND",
                    "No account found for this email address.");
            }

            // ── STEP 3: Already verified? ────────────────────────────
            if (user.EmailVerified)
            {
                return ResendOtpResult.Failure(
                    409,
                    "account verified before",
                    "EMAIL_ALREADY_VERIFIED",
                    "This email address has already been verified.");
            }

            // ── STEP 4: OTP + Rate limit ─────────────────────────────
            var now = DateTime.UtcNow;
            var existingOtp = await _emailOtpRepository.GetLatestByEmailAsync(email, ct);

            if (existingOtp is null)
            {
                await CreateAndSendOtpAsync(email, now, ct);
            }
            else
            {
                var windowStart = existingOtp.UpdatedAt ?? existingOtp.CreatedAt;
                var windowElapsed = (now - windowStart).TotalHours > _rateLimitWindowHours;

                if (!windowElapsed && existingOtp.Count >= _rateLimitMaxAttempts)
                {
                    return ResendOtpResult.Failure(
                        429,
                        "reach limitation of generation",
                        "OTP_RATE_LIMIT_EXCEEDED",
                        $"OTP limit {_rateLimitMaxAttempts}/{_rateLimitWindowHours}h exceeded.");
                }

                var newCount = windowElapsed ? 1 : existingOtp.Count + 1;

                await UpdateAndSendOtpAsync(
                    existingOtp.Id,
                    email,
                    now,
                    newCount,
                    ct);
            }

            return ResendOtpResult.Ok(new ResendOtpResponseDto
            {
                Email = email
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>Generates a new OTP, persists a fresh record, and fires the email.</summary>
        private async Task CreateAndSendOtpAsync(
            string email,
            DateTime now,
            CancellationToken ct)
        {
            var rawOtp = OtpHelper.Generate();


            var otpEntity = new EmailOtp
            {
                Id = Guid.NewGuid(),
                Email = email,
                OtpCode = rawOtp,
                ExpiresAt = now.AddMinutes(OtpExpiryMinutes),
                IsUsed = false,
                Count = 1,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _emailOtpRepository.CreateAsync(otpEntity, ct);
            await _emailOtpRepository.SaveChangesAsync(ct);

            _ = SendOtpFireAndForgetAsync(email, rawOtp);
        }

        /// <summary>
        /// Replaces the OTP hash in an existing record (in-place update)
        /// and fires the email.
        /// </summary>
        private async Task UpdateAndSendOtpAsync(
            Guid otpId,
            string email,
            DateTime now,
            int newCount,
            CancellationToken ct)
        {
            var rawOtp = OtpHelper.Generate();
            var newExpiresAt = now.AddMinutes(OtpExpiryMinutes);

            // ExecuteUpdateAsync commits immediately — no SaveChangesAsync needed.
            await _emailOtpRepository.UpdateAsync(
                otpId, rawOtp, newExpiresAt, newCount, now, ct);

            _ = SendOtpFireAndForgetAsync(email, rawOtp);
        }

        /// <summary>
        /// Sends the plain-text OTP email without blocking the API response.
        /// Failures are logged but do not surface to the caller — the user can retry.
        /// </summary>
        private async Task SendOtpFireAndForgetAsync(string email, string rawOtp)
        {
            try
            {
                await _emailService.SendOtpAsync(email, rawOtp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OTP email delivery failed for {Email}.", email);
            }
        }
    }
}
