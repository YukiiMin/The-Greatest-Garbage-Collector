using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Services
{
    /// <summary>
    /// Handles the password-reset OTP generation flow.
    ///
    /// Step-by-step (mirrors the API specification exactly):
    ///   1. Validate email format
    ///   2. Verify the user account exists in the database
    ///   3. Generate a cryptographically random 6-digit OTP
    ///   4. Upsert the password_otp record:
    ///        CASE A — no existing record → INSERT with count = 1
    ///        CASE B — record exists      → UPDATE (replace hash, reset expiry,
    ///                                      set is_used = false, increment count,
    ///                                      refresh last_updated_at)
    ///   5. Send plain-text OTP via email (fire-and-forget — never blocks response)
    ///
    /// Security:
    ///   • Only the BCrypt hash of the OTP is written to the DB — plain-text is
    ///     emailed only and is never accessible after the send call returns.
    ///   • Email is normalised (trim + toLowerInvariant) before any DB query or
    ///     persistence to prevent case-bypass attacks.
    ///   • No password validation is performed — this is an unauthenticated
    ///     public endpoint that only requires a valid email address.
    /// </summary>
    public sealed class PasswordOtpService : IPasswordOtpService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordOtpRepository _passwordOtpRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordOtpService> _logger;

        private const int OtpExpiryMinutes = 5;

        public PasswordOtpService(
            IUserRepository userRepository,
            IPasswordOtpRepository passwordOtpRepository,
            IEmailService emailService,
            ILogger<PasswordOtpService> logger)
        {
            _userRepository = userRepository;
            _passwordOtpRepository = passwordOtpRepository;
            _emailService = emailService;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────

        public async Task<PasswordOtpResult> CreatePasswordOtpAsync(
            CreatePasswordOtpRequestDto request,
            CancellationToken ct = default)
        {
            // ── STEP 1: Validate email format ─────────────────────────────────
            if (!ValidationHelper.IsValidEmail(request.Email))
            {
                return PasswordOtpResult.Failure(
                    statusCode: 422,
                    message: "wrong email format",
                    code: "INVALID_EMAIL_FORMAT",
                    description: "Email is not in correct format.");
            }

            var normalisedEmail = request.Email.Trim().ToLowerInvariant();

            // ── STEP 2: Check user exists ─────────────────────────────────────
            var user = await _userRepository.GetByEmailAsync(normalisedEmail, ct);
            if (user is null)
            {
                _logger.LogWarning(
                    "Password OTP requested for unknown email: {Email}.", normalisedEmail);

                return PasswordOtpResult.Failure(
                    statusCode: 404,
                    message: "account not found",
                    code: "ACCOUNT_NOT_FOUND",
                    description: "No account associated with this email.");
            }

            // ── STEP 3: Generate OTP ──────────────────────────────────────────
            var now = DateTime.UtcNow;
            var rawOtp = OtpHelper.Generate();
            var hashedOtp = PasswordHelper.Hash(rawOtp);     // BCrypt — never store plain-text
            var expiresAt = now.AddMinutes(OtpExpiryMinutes);

            // ── STEP 4: Upsert password_otp record ────────────────────────────
            var existing = await _passwordOtpRepository.GetByEmailAsync(normalisedEmail, ct);

            if (existing is null)
            {
                // CASE A: First-time reset request — INSERT
                var otpEntity = new PasswordOtp
                {
                    Email = normalisedEmail,
                    OtpCode = hashedOtp,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    Count = 1,
                    CreatedAt = now,
                    LastUpdatedAt = now
                };

                await _passwordOtpRepository.CreateAsync(otpEntity, ct);
                await _passwordOtpRepository.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Password OTP created (first request) for {Email}.", normalisedEmail);
            }
            else
            {
                // CASE B: Subsequent request — UPDATE in-place
                // ExecuteUpdateAsync commits immediately; no SaveChangesAsync needed.
                var newCount = existing.Count + 1;

                await _passwordOtpRepository.UpdateAsync(
                    email: normalisedEmail,
                    newOtpCodeHash: hashedOtp,
                    newExpiresAt: expiresAt,
                    newCount: newCount,
                    lastUpdatedAt: now,
                    ct: ct);

                _logger.LogInformation(
                    "Password OTP updated (attempt #{Count}) for {Email}.",
                    newCount, normalisedEmail);
            }

            // ── STEP 5: Send OTP by email (fire-and-forget) ───────────────────
            // The response is never blocked by email delivery.
            // If sending fails the error is logged; the user can request again.
            _ = SendOtpFireAndForgetAsync(normalisedEmail, rawOtp);

            return PasswordOtpResult.Ok(
                new CreatePasswordOtpResponseDto { Email = normalisedEmail });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task SendOtpFireAndForgetAsync(string email, string rawOtp)
        {
            try
            {
                await _emailService.SendOtpAsync(email, rawOtp);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Password OTP email delivery failed for {Email}.", email);
            }
        }
    }
}
