using GarbageCollection.API.Helpers;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Business.Services;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Auth;
using GarbageCollection.Common.DTOs.Auth.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Requests.BatchRequest;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IVerifyEmailService _verifyEmailService;
        private readonly ILocalAuthService _localAuthService;
        private readonly ILocalLoginService _localLoginService;
        private readonly IPasswordOtpService _passwordOtpService;
        private readonly IResendOtpService _resendOtpService;
        private readonly IAccountVerificationService _accountVerificationService;
        private readonly IAdminService _adminService;

        public AuthController(IAuthService authService, IConfiguration configuration, IVerifyEmailService verifyEmailService, ILocalLoginService localLoginService, IPasswordOtpService passwordOtpService, ILocalAuthService localAuthService, IResendOtpService resendOtpService, IAccountVerificationService accountVerificationService, IAdminService adminService)


        {
            _authService = authService;
            _configuration = configuration;
            _verifyEmailService = verifyEmailService;
            _localLoginService = localLoginService;
            _localAuthService = localAuthService;
            _passwordOtpService = passwordOtpService;
            _adminService = adminService;
            _resendOtpService = resendOtpService;
            _accountVerificationService = accountVerificationService;


        }

        /// <summary>
        /// POST /api/v1/auth/google-auth/account
        /// Validates a Google ID token and returns user profile + auth cookies.
        /// </summary>
        [HttpPost("google-auth/account")]
        [ProducesResponseType(typeof(ApiResponse<GoogleLoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GoogleLogin(
            [FromBody] GoogleLoginRequestWrapper request,
            CancellationToken ct)
        {
            if (request?.Data?.GoogleToken is null or { Length: 0 })
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "google_token is required",
                    "VALIDATION_ERROR",
                    "The google_token field must not be empty."));
            }

            var result = await _authService.GoogleLoginAsync(request.Data.GoogleToken, ct);

            if (!result.Succeeded)
            {
                var failBody = ApiResponse<object>.Fail(
                    result.FailMessage!,
                    result.FailCode!,
                    result.FailDescription!);

                return StatusCode(result.HttpStatusCode, failBody);
            }

            // STEP 5 – set HttpOnly cookies (controller concern, not service concern)
            CookieHelper.SetAuthCookies(Response, result.AccessToken!, result.RefreshToken!, _configuration);

            return Ok(ApiResponse<GoogleLoginResponseDto>.Success(
                "account is valid",
                result.Payload!));
        }

        // ───────────────── REGISTER LOCAL ─────────────────
        [HttpPost("local-auth/account-registration")]
        public async Task<IActionResult> Register(
            [FromBody] LocalRegisterRequestWrapper request)
        {
            var result = await _localAuthService.RegisterAsync(request.Data);

            if (!result.Succeeded)
            {
                return StatusCode(result.HttpStatusCode,
                    ApiResponse<object>.Fail(
                        result.FailMessage!,
                        result.FailCode!,
                        result.FailDescription!));
            }

            // 🍪 set cookies
            SetAuthCookies(result.AccessToken!, result.RefreshToken!);

            return Ok(ApiResponse<LocalRegisterResponseDto>.Success(
                "account is created",
                result.Payload!));
        }

        // ───────────────── HELPER ─────────────────
        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15) // match JwtHelper
            });

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }

        [Authorize]
        [HttpGet("account-auth/verification")]
        [ProducesResponseType(typeof(ApiResponse<AccountVerificationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AccountVerification(CancellationToken ct)
        {
            // ── Extract validated claims (HTTP / controller concern) ───────────
            // [Authorize] + JWT middleware guarantee these claims are authentic;
            // we only need to check for their presence, not re-validate the token.
            var tokenEmail = User.GetEmail();
            if (tokenEmail is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "failed verification",
                    "UNAUTHORIZED",
                    "Access token does not contain a valid email claim."));
            }

            var tokenLoginTerm = User.GetLoginTerm();

            // ── Delegate ALL business logic to the service ────────────────────
            var result = await _accountVerificationService
                .VerifyAccountAsync(tokenEmail, tokenLoginTerm, ct);

            if (!result.Succeeded)
            {
                return StatusCode(
                    result.HttpStatusCode,
                    ApiResponse<object>.Fail(
                        result.FailMessage!,
                        result.FailCode!,
                        result.FailDescription!));
            }

            return Ok(ApiResponse<AccountVerificationResponseDto>.Ok(
                result.Payload!,
                "you has been verified"
                ));
        }

        // ── POST /api/v1/auth/local-auth/login ───────────────────────────────

        /// <summary>
        /// Authenticates a local (email + password) user and issues auth cookies.
        /// </summary>
        /// <remarks>
        /// **Business rules enforced by the service**
        /// - Email must be valid format; password must meet complexity rules → 422
        /// - User must exist and password must match → 409 INVALID_CREDENTIALS
        ///   (same error for both cases — prevents user-enumeration)
        /// - Email must be verified before login is allowed → 409 EMAIL_NOT_VERIFIED
        ///
        /// **On success**
        /// - Two HttpOnly, SameSite=Strict cookies are set: `accessToken`, `refreshToken`
        /// - Response body contains the user profile (no password, no tokens)
        ///
        /// **Error codes**
        /// - `VALIDATION_ERROR` (422) — email or password format invalid
        /// - `INVALID_CREDENTIALS` (409) — email not found or password mismatch
        /// - `EMAIL_NOT_VERIFIED` (409) — account exists but email not confirmed yet
        /// </remarks>
        [HttpPost("local-auth/login")]
        [ProducesResponseType(typeof(ApiResponse<LocalLoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> LocalLogin(
            [FromBody] LocalLoginRequestWrapper request,
            CancellationToken ct)
        {
            // ── Shape guard ───────────────────────────────────────────────────
            if (request?.Data is null)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "data is unvalid",
                    "VALIDATION_ERROR",
                    "Request body must contain a 'data' object with email and password."));
            }

            // ── Delegate ALL business logic to the service ────────────────────
            var result = await _localLoginService.LoginAsync(request.Data, ct);

            if (!result.Succeeded)
            {
                return StatusCode(
                    result.HttpStatusCode,
                    ApiResponse<object>.Fail(
                        result.FailMessage!,
                        result.FailCode!,
                        result.FailDescription!));
            }

            // ── Set HttpOnly cookies (HTTP / controller concern) ───────────────
            CookieHelper.SetAuthCookies(
                Response, result.AccessToken!, result.RefreshToken!, _configuration);

            return Ok(ApiResponse<LocalLoginResponseDto>.Success(
                "account is valid",
                result.Payload!));
        }
        //[Authorize]
        [HttpPost("local-auth/account-verification")]
        [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> VerifyEmail(
           [FromBody] VerifyEmailRequestWrapper request,
           CancellationToken ct)
        {
            // ── Shape guard (malformed / missing body) ────────────────────────
            if (request?.Data is null)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "data is unvalid",
                    "INVALID_DATA",
                    "Request body must contain a 'data' object with email and otp."));
            }

            // ── Extract email from JWT claims (HTTP / security concern) ────────
            // [Authorize] guarantees User is authenticated at this point.
            // If the claim is somehow absent the token is malformed — treat as 401.
            var tokenEmail = User.GetEmail();
            if (tokenEmail is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "Unauthorized",
                    "UNAUTHORIZED",
                    "Access token does not contain a valid email claim."));
            }

            // ── Delegate ALL business logic to the service ────────────────────
            var result = await _verifyEmailService.VerifyEmailAsync(request.Data, tokenEmail, ct);

            if (!result.Succeeded)
            {
                return StatusCode(
                    result.HttpStatusCode,
                    ApiResponse<object>.Fail(
                        result.FailMessage!,
                        result.FailCode!,
                        result.FailDescription!));
            }

            return Ok(ApiResponse<VerifyEmailResponseDto>.Success(
                "account has been verified",
                result.Payload!));
        }

       
        [HttpPost("local-auth/email-otp")]
        [ProducesResponseType(typeof(ApiResponse<ResendOtpResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ResendOtp(
            [FromBody] ResendOtpRequestWrapper request,
            CancellationToken ct)
        {
            // ── Shape guard ───────────────────────────────────────────────────
            if (request?.Data is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "data is unvalid",
                    "INVALID_DATA",
                    "Request body must contain a 'data' object with an email field."));
            }

            // ── Extract validated JWT claim (HTTP concern — controller only) ───
            // [Authorize] + middleware guarantee the token is authentic before we reach here.

            // ── Delegate ALL business logic to the service ────────────────────
            var result = await _resendOtpService.ResendOtpAsync(request.Data, ct);

            if (!result.Succeeded)
            {
                return StatusCode(
                    result.HttpStatusCode,
                    ApiResponse<object>.Fail(result.FailMessage!, result.FailCode!, result.FailDescription!));
            }

            return Ok(ApiResponse<ResendOtpResponseDto>.Ok(result.Payload!, "otp has been created"));
        }
        [HttpGet("account-auth/license")]
        [ProducesResponseType(typeof(ApiResponse<LicenseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> License(CancellationToken ct)
        {
            // ── Read the raw refresh token JWT from the HttpOnly cookie ────────
            // This is the only HTTP concern in this action.
            // The cookie value is the signed JWT produced by JwtHelper.GenerateRefreshToken.
            // Null means the cookie is absent (step 1 of the spec).
            Request.Cookies.TryGetValue("refreshToken", out var refreshTokenJwt);

            // ── Delegate ALL logic to the service ─────────────────────────────
            var result = await _authService.IssueLicenseAsync(refreshTokenJwt, ct);

            if (!result.Succeeded)
            {
                return StatusCode(
                    result.HttpStatusCode,
                    ApiResponse<object>.Fail(result.FailMessage!, result.FailCode!, result.FailDescription!));
            }

            // ── Set new HttpOnly cookies (HTTP concern — controller only) ──────
            CookieHelper.SetAuthCookies(
                Response, result.AccessToken!, result.RefreshToken!, _configuration);

            return Ok(ApiResponse<LicenseResponseDto>.Success(
                "you has supplied license",
                result.Payload!));
        }

        [HttpPost("local-auth/password-otp")]
        [ProducesResponseType(typeof(ApiResponse<CreatePasswordOtpResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreatePasswordOtp(
           [FromBody] CreatePasswordOtpRequestWrapper request,
           CancellationToken ct)
        {
            // ── Shape guard ───────────────────────────────────────────────────
            if (request?.Data is null)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "wrong email format",
                    "INVALID_EMAIL_FORMAT",
                    "Request body must contain a 'data' object with an email field."));
            }

            // ── Delegate ALL business logic to the service ────────────────────
            var result = await _passwordOtpService.CreatePasswordOtpAsync(request.Data, ct);

            if (!result.Succeeded)
            {
                return StatusCode(
                    result.HttpStatusCode,
                    ApiResponse<object>.Fail(result.FailMessage!, result.FailCode!, result.FailDescription!));
            }

            return Ok(ApiResponse<CreatePasswordOtpResponseDto>.Success(
                "otp created successfully", result.Payload!));
        }
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestDto request,
        CancellationToken ct)
        {
            var (statusCode, response) = await _authService.ResetPasswordAsync(request, ct);
            return StatusCode(statusCode, response);
        }

        
    }

}





    



