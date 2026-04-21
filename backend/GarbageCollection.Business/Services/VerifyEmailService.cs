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
    /// Handles the email OTP verification flow.
    ///
    /// Step-by-step (mirrors the API specification exactly):
    ///   1.  Validate email format
    ///   2.  Extract email from JWT access token          ← done by controller, passed in
    ///   3.  Compare request email with token email
    ///   4.  Retrieve user from database
    ///   5.  Check EmailVerified == true                  → 409
    ///   6.  Retrieve latest OTP from email_otp table
    ///   7.  Check OTP exists                             → 400
    ///   8.  Check OTP.IsUsed                             → 400
    ///   9.  Check OTP expiration (ExpiresAt vs UtcNow)  → 400
    ///   10. Compare OTP via BCrypt (PasswordHelper.Verify) → 409
    ///   11. Mark OTP as used + set EmailVerified = true + save
    /// </summary>
    public sealed class VerifyEmailService : IVerifyEmailService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailOtpRepository _emailOtpRepository;
        private readonly ILogger<VerifyEmailService> _logger;

        public VerifyEmailService(
            IUserRepository userRepository,
            IEmailOtpRepository emailOtpRepository,
            ILogger<VerifyEmailService> logger)
        {
            _userRepository = userRepository;
            _emailOtpRepository = emailOtpRepository;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────

        public async Task<VerifyEmailResult> VerifyEmailAsync(
            VerifyEmailRequestDto request,
            string tokenEmail,
            CancellationToken ct = default)
        {
            // ── STEP 1: Validate email format ─────────────────────────────────
            if (!ValidationHelper.IsValidEmail(request.Email))
            {
                return VerifyEmailResult.Failure(
                    statusCode: 422,
                    message: "data is unvalid",
                    code: "INVALID_DATA",
                    description: "Email format is invalid or empty.");
            }

            // ── STEP 3: Compare request email with token email ────────────────
            // Normalise both sides before comparing to prevent case-sensitivity issues.
            var normalisedRequestEmail = request.Email.Trim().ToLowerInvariant();
            var normalisedTokenEmail = tokenEmail.Trim().ToLowerInvariant();

            if (normalisedRequestEmail != normalisedTokenEmail)
            {
                _logger.LogWarning(
                    "Email mismatch — token: {TokenEmail}, request: {RequestEmail}",
                    normalisedTokenEmail, normalisedRequestEmail);

                return VerifyEmailResult.Failure(
                    statusCode: 422,
                    message: "data is unvalid",
                    code: "INVALID_DATA",
                    description: "Request email does not match the authenticated token.");
            }

            // ── STEP 4: Retrieve user from database ───────────────────────────
            // Use tracking query so we can mutate EmailVerified and save in one round-trip.
            var user = await _userRepository.GetByEmailTrackedAsync(normalisedRequestEmail, ct);

            if (user is null)
            {
                _logger.LogWarning(
                    "Verification attempted for unknown email: {Email}", normalisedRequestEmail);

                return VerifyEmailResult.Failure(
                    statusCode: 404,
                    message: "user not found",
                    code: "USER_NOT_FOUND",
                    description: "No account exists for this email address.");
            }

            // ── STEP 5: Already verified? ─────────────────────────────────────
            if (user.EmailVerified)
            {
                return VerifyEmailResult.Failure(
                    statusCode: 409,
                    message: "email already verified",
                    code: "EMAIL_ALREADY_VERIFIED",
                    description: "This email address has already been verified.");
            }

            // ── STEP 6: Retrieve latest OTP record ────────────────────────────
            var otp = await _emailOtpRepository.GetLatestByEmailAsync(normalisedRequestEmail, ct);

            // ── STEP 7: OTP existence check ───────────────────────────────────
            if (otp is null)
            {
                return VerifyEmailResult.Failure(
                    statusCode: 400,
                    message: "otp is invalid",
                    code: "OTP_INVALID",
                    description: "No OTP was found for this email address.");
            }

            // ── STEP 8: Already used? ─────────────────────────────────────────
            if (otp.IsUsed)
            {
                return VerifyEmailResult.Failure(
                    statusCode: 400,
                    message: "otp is invalid",
                    code: "OTP_ALREADY_USED",
                    description: "This OTP has already been used.");
            }

            // ── STEP 9: Expiry check ──────────────────────────────────────────
            if (DateTime.UtcNow > otp.ExpiresAt)
            {
                _logger.LogInformation(
                    "Expired OTP submission for {Email}. Expired at {ExpiresAt}.",
                    normalisedRequestEmail, otp.ExpiresAt);

                return VerifyEmailResult.Failure(
                    statusCode: 400,
                    message: "otp is invalid",
                    code: "OTP_EXPIRED",
                    description: "The OTP has expired. Please request a new one.");
            }

            // ── STEP 10: OTP value comparison (BCrypt verify) ─────────────────
            // otp.OtpCode is the BCrypt hash stored at registration time.
            // request.Otp is the plain-text code the user typed in.
            if (request.Otp != otp.OtpCode)
            {
                _logger.LogWarning(
                    "OTP mismatch for {Email}.", normalisedRequestEmail);

                return VerifyEmailResult.Failure(
                    statusCode: 409,
                    message: "otp does not match",
                    code: "OTP_MISMATCH",
                    description: "The OTP you entered is incorrect.");
            }

            // ── STEP 11: Commit — mark OTP used + verify email ────────────────
            // MarkUsedAsync uses ExecuteUpdateAsync (direct SQL UPDATE, no entity load).
            await _emailOtpRepository.MarkUsedAsync(otp.Id, ct);

            // user is change-tracked (loaded with GetByEmailTrackedAsync).
            // EF Core detects these assignments and issues an UPDATE on SaveChangesAsync.
            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Email verified successfully for {Email}.", normalisedRequestEmail);

            return VerifyEmailResult.Ok(new VerifyEmailResponseDto { Email = user.Email });
        }
    }
}
