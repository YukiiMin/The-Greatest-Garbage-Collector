using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.User;
using GarbageCollection.Common.DTOs.Leaderboard;
using GarbageCollection.Common.Enums;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IUploadImageService _uploadImageService;
        private readonly ILeaderboardService _leaderboardService;

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private const long MaxAvatarSizeBytes = 5 * 1024 * 1024; // 5 MB

        public UsersController(IUserService userService, IUserRepository userRepository, IUploadImageService uploadImageService, ILeaderboardService leaderboardService)
        {
            _userService        = userService;
            _userRepository     = userRepository;
            _uploadImageService = uploadImageService;
            _leaderboardService = leaderboardService;
        }

        /// <summary>
        /// Lấy thông tin profile của user đang đăng nhập.
        /// </summary>
        [Authorize]
        [HttpGet("/api/v1/users/profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            var result = await _userService.GetProfileAsync(userId);
            return Ok(ApiResponse<UserProfileDto>.Ok(result, "get user profile successfully"));
        }

        /// <summary>
        /// Cập nhật thông tin profile của user đang đăng nhập.
        /// </summary>
        [Authorize]
        [HttpPut("/api/v1/users/profile")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413RequestEntityTooLarge)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request, IFormFile? avatar)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT"));

            string? avatarUrl = null;
            if (avatar != null)
            {
                var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                    return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_FILE_FORMAT",
                        $"Định dạng không hợp lệ: {ext}. Chỉ chấp nhận jpg, jpeg, png."));

                if (avatar.Length > MaxAvatarSizeBytes)
                    return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                        ApiResponse<object>.Fail("file too large", "FILE_TOO_LARGE", "Ảnh đại diện tối đa 5MB."));

                avatarUrl = await _uploadImageService.UploadImageAsync(avatar, "avatars");
            }

            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            var result = await _userService.UpdateProfileAsync(userId, request, avatarUrl);
            return Ok(ApiResponse<UserProfileDto>.Ok(result, "update user profile successfully"));
        }

        /// <summary>
        /// Đổi mật khẩu. Cấp lại accessToken mới qua cookie sau khi đổi thành công.
        /// </summary>
        [Authorize]
        [HttpPut("/api/v1/users/profile/change-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            string newAccessToken;
            try
            {
                newAccessToken = await _userService.ChangePasswordAsync(email, request.Data, ct);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("old password is incorrect"))
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message, "INVALID_OLD_PASSWORD"));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("new password must be different"))
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message, "SAME_PASSWORD"));
            }
            catch (ArgumentException ex)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid password format", "INVALID_PASSWORD_FORMAT", ex.Message));
            }

            Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.Strict,
                Expires  = DateTime.UtcNow.AddMinutes(15)
            });

            return Ok(ApiResponse<object>.Ok(null!, "password updated successfully"));
        }

        /// <summary>
        /// Xem bảng xếp hạng điểm theo tuần/tháng/năm.
        /// </summary>
        [Authorize]
        [HttpGet("/api/v1/users/leaderboard")]
        [ProducesResponseType(typeof(ApiResponse<LeaderboardResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetLeaderboard(
            [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.Week,
            [FromQuery] LeaderboardScope scope = LeaderboardScope.Ward,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken ct = default)
        {
            if (page < 1 || limit < 1 || limit > 50)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid query params", "INVALID_QUERY_PARAMS",
                    "page >= 1, limit must be between 1 and 50"));

            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            var result = await _leaderboardService.GetLeaderboardAsync(userId, period, scope, page, limit, ct);
            return Ok(ApiResponse<LeaderboardResult>.Ok(result, "get leaderboard successfully"));
        }

        private async Task<(Guid Id, IActionResult? Error)> GetAuthorizedUserAsync()
        {
            var email = User.GetEmail();
            if (email is null)
                return (Guid.Empty, Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED")));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return (Guid.Empty, NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND")));

            return (user.Id, null);
        }
    }
}
