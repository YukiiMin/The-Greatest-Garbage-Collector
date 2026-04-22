using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs.Auth.Local;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
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
    /// Handles the full local-registration flow:
    ///   1. Validate email + password
    ///   2. Check for duplicate email
    ///   3. Create user (BCrypt-hashed password)
    ///   4. Generate OTP → persist → send email  (fire-and-forget on send failure)
    ///   5. Generate JWT pair → persist refresh token hash
    /// </summary>
    public sealed class LocalAuthService : ILocalAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEmailOtpRepository _emailOtpRepository;
        private readonly IEmailService _emailService;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<LocalAuthService> _logger;

        private const int OtpExpiryMinutes = 5;

        public LocalAuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IEmailOtpRepository emailOtpRepository,
            IEmailService emailService,
            JwtHelper jwtHelper,
            ILogger<LocalAuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _emailOtpRepository = emailOtpRepository;
            _emailService = emailService;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────

        public async Task<RegisterResult> RegisterAsync(
            LocalRegisterRequestDto request,
            CancellationToken ct = default)
        {
            // ── STEP 1: Validate input ────────────────────────────────────────
            if (!ValidationHelper.IsValidEmail(request.Email))
            {
                return RegisterResult.Failure(
                    statusCode: 422,
                    message: "data is unvalid",
                    code: "INVALID_DATA",
                    description: "Email format is invalid or empty.");
            }

            var passwordError = ValidationHelper.GetPasswordValidationError(request.Password);
            if (passwordError is not null)
            {
                return RegisterResult.Failure(
                    statusCode: 422,
                    message: "data is unvalid",
                    code: "INVALID_DATA",
                    description: passwordError);
            }

            // ── STEP 2: Duplicate email check ────────────────────────────────
            var existing = await _userRepository.GetByEmailAsync(request.Email, ct);
            if (existing is not null)
            {
                _logger.LogWarning("Registration attempt for existing email: {Email}", request.Email);
                return RegisterResult.Failure(
                    statusCode: 409,
                    message: "account existed",
                    code: "ACCOUNT_EXISTS",
                    description: "Email already exists.");
            }

            // ── STEP 3: Create user ───────────────────────────────────────────
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = PasswordHelper.Hash(request.Password),
                FullName = request.FullName.Trim(),
                Provider = "local",
                EmailVerified = false,
                IsBanned = false,
                IsLogin = true,
                LoginTerm = 0,
                Role = UserRole.Citizen,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user, ct);
            await _userRepository.SaveChangesAsync(ct);
            _logger.LogInformation("New local user created: {Email}", user.Email);

            // ── STEP 4: OTP flow ──────────────────────────────────────────────
            var otpCode = OtpHelper.Generate();

            var otpEntity = new EmailOtp
            {
                Id = Guid.NewGuid(),
                Email = user.Email,
                OtpCode = otpCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _emailOtpRepository.CreateAsync(otpEntity, ct);
            await _emailOtpRepository.SaveChangesAsync(ct);

            // Send email without blocking the registration response.
            // If the send fails, the user can request a new OTP later.
            _ = SendOtpFireAndForgetAsync(user.Email, otpCode);

            // ── STEP 5: Generate JWT pair ─────────────────────────────────────
            var (accessToken, _) =
                _jwtHelper.GenerateAccessToken(user.Email, user.FullName, user.LoginTerm);

            var (rawRefresh, refreshJwt, refreshExpiry) =
                _jwtHelper.GenerateRefreshToken(user.Email);

            // Revoke any stale tokens (edge case: prior abandoned registrations)
            await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);

            var tokenHash = JwtHelper.HashToken(rawRefresh);
            var refreshEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                Email = user.Email,
                ExpiresAt = refreshExpiry,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.CreateAsync(refreshEntity, ct);
            await _refreshTokenRepository.SaveChangesAsync(ct);

            // ── STEP 6: Return success payload ────────────────────────────────
            return RegisterResult.Ok(
                new LocalRegisterResponseDto { Email = user.Email },
                accessToken,
                refreshJwt);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private async Task SendOtpFireAndForgetAsync(string email, string otpCode)
        {
            try
            {
                await _emailService.SendOtpAsync(email, otpCode);
            }
            catch (Exception ex)
            {
                // Non-fatal: user can trigger a resend. Log for ops visibility.
                _logger.LogError(ex, "OTP email delivery failed for {Email}.", email);
            }
        }
    }
}
