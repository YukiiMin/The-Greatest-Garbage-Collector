using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Auth;
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
    /// Handles the full local-login flow.
    ///
    /// Step-by-step (mirrors the API specification exactly):
    ///   1. Validate email format + password format rules
    ///   2. Query user by email; if absent → 409 INVALID_CREDENTIALS
    ///   3. BCrypt-verify the supplied password against the stored hash;
    ///      if mismatch → 409 INVALID_CREDENTIALS
    ///      (same code as "not found" — prevents user-enumeration attacks)
    ///   4. Verify email is confirmed; if not → 409 EMAIL_NOT_VERIFIED
    ///   5. Generate access token + refresh token (reuses JwtHelper)
    ///   6. Revoke all previous refresh tokens, persist the new hash
    ///   7. Return LoginResult.Ok with user profile payload + token strings
    ///      (cookies are set by the controller — HTTP concern)
    /// </summary>
    public sealed class LocalLoginService : ILocalLoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<LocalLoginService> _logger;

        public LocalLoginService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            JwtHelper jwtHelper,
            ILogger<LocalLoginService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────

        public async Task<LoginResult> LoginAsync(
            LocalLoginRequestDto request,
            CancellationToken ct = default)
        {
            // ── STEP 1: Validate input format ─────────────────────────────────
            if (!ValidationHelper.IsValidEmail(request.Email))
            {
                return LoginResult.Failure(
                    statusCode: 422,
                    message: "data is unvalid",
                    code: "VALIDATION_ERROR",
                    description: "Invalid email or password format.");
            }

            if (!ValidationHelper.IsValidPassword(request.Password))
            {
                return LoginResult.Failure(
                    statusCode: 422,
                    message: "data is unvalid",
                    code: "VALIDATION_ERROR",
                    description: "Invalid email or password format.");
            }

            // ── STEP 2: Lookup user ───────────────────────────────────────────
            // Normalise email to match how it was stored at registration time.
            var normalisedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalisedEmail, ct);

            // Return the SAME error for "not found" and "wrong password" to
            // prevent user-enumeration attacks.
            if (user is null)
            {
                _logger.LogWarning(
                    "Login attempt for unknown email: {Email}", normalisedEmail);

                return LoginResult.Failure(
                    statusCode: 409,
                    message: "account is incorrect",
                    code: "INVALID_CREDENTIALS",
                    description: "Email or password is incorrect.");
            }

            // ── STEP 3: Verify password ───────────────────────────────────────
            // Guard against null hash (e.g. Google-only accounts that have no password).
            var passwordValid = user.PasswordHash is not null
                                && PasswordHelper.Verify(request.Password, user.PasswordHash);

            if (!passwordValid)
            {
                _logger.LogWarning(
                    "Failed password attempt for email: {Email}", normalisedEmail);

                return LoginResult.Failure(
                    statusCode: 409,
                    message: "account is incorrect",
                    code: "INVALID_CREDENTIALS",
                    description: "Email or password is incorrect.");
            }

            // ── STEP 4: Check email verified ──────────────────────────────────
            if (!user.EmailVerified)
            {
                _logger.LogInformation(
                    "Login blocked — email not verified: {Email}", normalisedEmail);

                return LoginResult.Failure(
                    statusCode: 409,
                    message: "account has not been verified",
                    code: "EMAIL_NOT_VERIFIED",
                    description: "User must verify email before login.");
            }

            // ── STEP 5: Generate JWT pair ─────────────────────────────────────
            var (accessToken, _) =
                _jwtHelper.GenerateAccessToken(user.Email, user.FullName, user.LoginTerm);

            var (rawRefresh, refreshJwt, refreshExpiry) =
                _jwtHelper.GenerateRefreshToken(user.Email);

            // ── STEP 6: Persist new refresh token (revoke previous ones) ──────
            // Enforces single-session semantics: old refresh tokens are invalidated
            // on every fresh login, eliminating stale credential risk.
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

            _logger.LogInformation(
                "Successful local login for {Email}.", user.Email);

            // ── STEP 7: Return success payload ────────────────────────────────
            // Password hash and tokens are intentionally excluded from the response body.
            // Tokens travel exclusively via HttpOnly cookies set by the controller.
            return LoginResult.Ok(
                payload: new LocalLoginResponseDto
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    Address = user.Address
                },
                accessToken: accessToken,
                refreshToken: refreshJwt);
        }
    }
}
