using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Admin;
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
        private readonly IUserService        _userService;
        private readonly IUserRepository     _userRepository;
        private readonly IUploadImageService _uploadImageService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly IWorkAreaService    _workAreaService;

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private const long MaxAvatarSizeBytes = 5 * 1024 * 1024; // 5 MB

        public UsersController(
            IUserService        userService,
            IUserRepository     userRepository,
            IUploadImageService uploadImageService,
            ILeaderboardService leaderboardService,
            IWorkAreaService    workAreaService)
        {
            _userService        = userService;
            _userRepository     = userRepository;
            _uploadImageService = uploadImageService;
            _leaderboardService = leaderboardService;
            _workAreaService    = workAreaService;
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
        [HttpPatch("/api/v1/users/profile")]
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
        /// Cập nhật vị trí cư trú (ward + địa chỉ cụ thể). Chỉ dành cho Citizen.
        /// </summary>
        [Authorize]
        [HttpPatch("/api/v1/users/profile/location")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateCitizenLocationRequest request)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT"));

            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            try
            {
                var result = await _userService.UpdateLocationAsync(userId, request);
                return Ok(ApiResponse<UserProfileDto>.Ok(result, "location updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(ex.Message, "FORBIDDEN"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message, "NOT_FOUND"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message, "BAD_REQUEST"));
            }
        }

        /// <summary>
        /// Danh sách districts để citizen chọn khu vực.
        /// </summary>
        [Authorize]
        [HttpGet("/api/v1/users/locations/districts")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkAreaDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDistricts()
        {
            var districts = await _workAreaService.GetAllAsync("District");
            return Ok(ApiResponse<List<WorkAreaDto>>.Success("success", districts));
        }

        /// <summary>
        /// Danh sách wards thuộc một district.
        /// </summary>
        [Authorize]
        [HttpGet("/api/v1/users/locations/districts/{id}/wards")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkAreaDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWards([FromRoute] Guid id)
        {
            try
            {
                var district = await _workAreaService.GetByIdAsync(id);
                return Ok(ApiResponse<List<WorkAreaDto>>.Success("success", district.Children));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message, "NOT_FOUND"));
            }
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

            try
            {
                var result = await _leaderboardService.GetLeaderboardAsync(userId, period, scope, page, limit, ct);
                return Ok(ApiResponse<LeaderboardResult>.Ok(result, "get leaderboard successfully"));
            }
            catch (InvalidOperationException ex) when (ex.Message == "WORK_AREA_NOT_SET")
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "work area not set", "WORK_AREA_NOT_SET",
                    "Ban chua chon phuong cu tru. Vui long cap nhat profile truoc khi xem leaderboard theo khu vuc."));
            }
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
