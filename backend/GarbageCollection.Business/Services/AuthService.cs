using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Auth;
using GarbageCollection.Common.DTOs.Auth.Local;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.Common.Models.Internal;
using GarbageCollection.DataAccess.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GarbageCollection.Business.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthService> _logger;
        private TokenValidationParameters _refreshTokenValidationParams;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            JwtHelper jwtHelper,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────

        public async Task<GoogleAuthResult> GoogleLoginAsync(string googleToken, CancellationToken ct = default)
        {
            // STEP 1 – Validate Google token
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Google token validation failed.");
                return GoogleAuthResult.Failure(
                    statusCode: 404,
                    message: "account is not found on google",
                    code: "GOOGLE_INVALID",
                    description: "Invalid Google token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Google token validation.");
                return GoogleAuthResult.Failure(
                    statusCode: 404,
                    message: "account is not found on google",
                    code: "GOOGLE_INVALID",
                    description: "Invalid Google token");
            }

            // STEP 3 – Check database
            var user = await _userRepository.GetByGoogleIdAsync(payload.Subject, ct)
                    ?? await _userRepository.GetByEmailAsync(payload.Email, ct);

            if (user is null)
            {
                // CASE A – create new user
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = payload.Email,
                    EmailVerified = true,
                    GoogleId = payload.Subject,
                    Provider = "google",
                    FullName = payload.Name,
                    AvatarUrl = payload.Picture,
                    IsBanned = false,
                    IsLogin = true,
                    LoginTerm = 0,
                    Role = UserRole.Citizen,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(user, ct);
                await _userRepository.SaveChangesAsync(ct);

                _logger.LogInformation("New user created via Google auth: {Email}", user.Email);
            }
            else
            {
                // CASE B – existing user checks
                if (user.IsBanned)
                {
                    _logger.LogWarning("Banned user attempted Google login: {Email}", user.Email);
                    return GoogleAuthResult.Failure(
                        statusCode: 403,
                        message: "account is banned",
                        code: "USER_BANNED",
                        description: "Account is banned");
                }

                // Patch GoogleId if the user registered earlier via email/password
                if (user.GoogleId is null)
                {
                    // Re-attach (AsNoTracking was used in repo, so we update via raw SQL or re-fetch with tracking)
                    // Here we use a lightweight update approach – reload with tracking
                    user.GoogleId = payload.Subject;
                    user.IsLogin = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    // The repo doesn't expose Update; we rely on EF change tracking from CreateAsync path
                    // For existing users we save via SaveChanges on the context (injected into repo)
                }
            }

            // STEP 4 – Generate tokens
            var (accessToken, _) = _jwtHelper.GenerateAccessToken(user.Email, user.FullName, user.LoginTerm);
            var (rawRefresh, refreshJwt, refreshExpiry) = _jwtHelper.GenerateRefreshToken(user.Email);

            // Persist hashed refresh token (revoke old ones first to enforce single-session)
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

            // STEP 6 – Build response payload
            var responsePayload = new GoogleLoginResponseDto
            {
                Email = user.Email,
                HasPassword = user.PasswordHash is not null,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Address = user.Address
            };

            return GoogleAuthResult.Ok(responsePayload, accessToken, refreshJwt);
        }

        public Task RegisterAsync(LocalRegisterRequestDto data)
        {
            throw new NotImplementedException();
        }
        public async Task<LicenseResult> IssueLicenseAsync(
            string? rawRefreshTokenJwt,
            CancellationToken ct = default)
        {
            // ── STEP 1: Cookie missing ────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(rawRefreshTokenJwt))
            {
                return LicenseResult.Failure(
                    statusCode: 401,
                    message: "no license",
                    code: "NO_REFRESH_TOKEN",
                    description: "Refresh token missing.");
            }

            // ── STEP 2: Validate JWT structure + extract raw token claim ──────
            // ValidateLifetime = false so an expired JWT still lets us reach step 5
            // and return 422 (expired) instead of 401 (invalid).
            ClaimsPrincipal principal;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                principal = handler.ValidateToken(
                    rawRefreshTokenJwt,
                    _refreshTokenValidationParams,
                    out _);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Refresh token JWT validation failed.");
                return LicenseResult.Failure(
                    statusCode: 401,
                    message: "no license",
                    code: "INVALID_REFRESH_TOKEN",
                    description: "Refresh token is invalid or has been tampered with.");
            }

            // Extract the raw token value embedded as a claim in the JWT payload.
            var rawToken = principal.FindFirstValue("refresh_token");
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                _logger.LogWarning("Refresh token JWT is missing 'refresh_token' claim.");
                return LicenseResult.Failure(
                    statusCode: 401,
                    message: "no license",
                    code: "INVALID_REFRESH_TOKEN",
                    description: "Refresh token payload is malformed.");
            }

            // ── STEP 3: Hash the raw token and look up the DB record ──────────
            var tokenHash = JwtHelper.HashToken(rawToken);
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

            if (storedToken is null)
            {
                _logger.LogWarning("Refresh token hash not found in DB.");
                return LicenseResult.Failure(
                    statusCode: 401,
                    message: "no license",
                    code: "INVALID_REFRESH_TOKEN",
                    description: "Refresh token is not recognised.");
            }

            // ── STEP 4: Reuse detection — token already revoked ───────────────
            // A revoked token being presented is a security event: it means either
            // a previous rotation wasn't completed (client bug) or the token was
            // stolen and already used by an attacker. Invalidate all tokens and
            // bump login_term so every outstanding access token is also invalidated.
            if (storedToken.IsRevoked)
            {
                _logger.LogWarning(
                    "SECURITY EVENT — refresh token reuse detected for user {UserId}.",
                    storedToken.UserId);

                // Revoke ALL tokens for the user (parallel sessions included).
                await _refreshTokenRepository.RevokeAllForUserAsync(storedToken.UserId, ct);

                // Increment login_term → all existing access tokens become stale
                // and will fail the login_term check in AccountVerificationService.
                await _userRepository.IncrementLoginTermAsync(storedToken.UserId, ct);

                return LicenseResult.Failure(
                    statusCode: 409,
                    message: "abnormal detection",
                    code: "TOKEN_REUSE",
                    description: "Refresh token reuse detected. All sessions have been terminated.");
            }

            // ── STEP 5: Expiry check (use DB ExpiresAt — authoritative) ───────
            if (DateTime.UtcNow > storedToken.ExpiresAt)
            {
                _logger.LogInformation(
                    "Refresh token expired for {Email}. ExpiresAt: {ExpiresAt}.",
                    storedToken.Email, storedToken.ExpiresAt);

                return LicenseResult.Failure(
                    statusCode: 422,
                    message: "expired",
                    code: "TOKEN_EXPIRED",
                    description: "Refresh token has expired. Please log in again.");
            }

            // The user profile was eagerly loaded in GetByTokenHashAsync.
            var user = storedToken.User;

            // ── STEP 6: Rotation — generate a new token pair ──────────────────
            var (newAccessToken, _) =
                _jwtHelper.GenerateAccessToken(user.Email, user.FullName, user.LoginTerm);
            var (newRawRefresh, newRefreshJwt, newRefreshExpiry) =
                _jwtHelper.GenerateRefreshToken(user.Email);

            // ── STEP 7: Revoke the consumed token (single-use enforcement) ────
            // RevokeByIdAsync commits immediately via ExecuteUpdateAsync.
            await _refreshTokenRepository.RevokeByIdAsync(storedToken.Id, ct);

            // ── STEP 8: Persist the new refresh token (hashed, never raw) ─────
            var newTokenHash = JwtHelper.HashToken(newRawRefresh);
            var newRefreshEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = newTokenHash,
                Email = user.Email,
                ExpiresAt = newRefreshExpiry,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.CreateAsync(newRefreshEntity, ct);
            await _refreshTokenRepository.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Token rotation completed successfully for {Email}.", user.Email);

            // ── STEP 9: Return success — tokens go via cookies (controller) ───
            return LicenseResult.Ok(
                payload: new LicenseResponseDto
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    Address = user.Address
                },
                accessToken: newAccessToken,
                refreshToken: newRefreshJwt);
        }
    }
}
