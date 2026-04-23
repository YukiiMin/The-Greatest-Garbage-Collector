using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.CitizenReport;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CitizenReportController : ControllerBase
    {
        private readonly ICitizenReportService _reportService;
        private readonly IUserRepository _userRepository;

        public CitizenReportController(ICitizenReportService reportService, IUserRepository userRepository)
        {
            _reportService   = reportService;
            _userRepository  = userRepository;
        }

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        /// <summary>
        /// Citizen gửi báo cáo rác mới (multipart/form-data).
        /// </summary>
        [Authorize]
        [HttpPost("/api/v1/users/citizen-reports")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CitizenReportResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413RequestEntityTooLarge)]
        public async Task<IActionResult> CreateReport([FromForm] CreateCitizenReportDto dto)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT"));

            if (dto.Images.Count < 1)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT", "Vui lòng gửi ít nhất 1 ảnh."));

            if (dto.Images.Count > 3)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT", "Tối đa 5 ảnh mỗi lần gửi."));

            if (dto.Types.Count < 1)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT", "Vui lòng chọn ít nhất 1 loại rác."));

            var invalidFormat = dto.Images.FirstOrDefault(f =>
                !AllowedImageExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()));
            if (invalidFormat != null)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_FILE_FORMAT",
                    $"Định dạng không hợp lệ: {Path.GetExtension(invalidFormat.FileName)}. Chỉ chấp nhận jpg, jpeg, png."));

            var oversized = dto.Images.FirstOrDefault(f => f.Length > MaxFileSizeBytes);
            if (oversized != null)
                return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                    ApiResponse<object>.Fail("file too large", "FILE_TOO_LARGE", "Mỗi ảnh tối đa 5MB."));

            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            var result = await _reportService.CreateReportAsync(userId, dto);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<CitizenReportResponseDto>.Ok(result, "report created successfully"));
        }

        /// <summary>
        /// Lấy chi tiết một báo cáo theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CitizenReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportById(int id)
        {
            var result = await _reportService.GetReportByIdAsync(id);
            if (result is null)
                return NotFound(ApiResponse<object>.Fail($"Không tìm thấy báo cáo với ID {id}.", "NOT_FOUND"));

            return Ok(ApiResponse<CitizenReportResponseDto>.Ok(result));
        }

        /// <summary>
        /// Lấy danh sách báo cáo của User đang đăng nhập, hỗ trợ phân trang.
        /// </summary>
        /// <param name="page">Trang hiện tại, bắt đầu từ 1 (mặc định: 1)</param>
        /// <param name="limit">Số bản ghi mỗi trang, tối đa 50 (mặc định: 10)</param>
        [Authorize]
        [HttpGet("/api/v1/users/citizen-reports")]
        [ProducesResponseType(typeof(ApiResponse<CitizenReportsResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetCitizenReports([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            if (page < 1 || limit < 1 || limit > 50)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid query params",
                    "INVALID_QUERY_PARAMS",
                    "page >= 1, limit must be between 1 and 50"));

            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            var result = await _reportService.GetUserReportsPagedAsync(userId, page, limit);
            return Ok(ApiResponse<CitizenReportsResult>.Ok(result, "get citizen reports successfully"));
        }

        /// <summary>
        /// Citizen cập nhật báo cáo — chỉ được khi status là Pending và chưa từng update.
        /// </summary>
        /// <param name="id">ID của báo cáo</param>
        /// <param name="dto">Các trường cần cập nhật (tất cả optional)</param>
        [Authorize]
        [HttpPut("/api/v1/users/citizen-reports/{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CitizenReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> UpdateReport(int id, [FromForm] UpdateCitizenReportDto dto)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT"));

            if (dto.Images != null && dto.Images.Count > 0)
            {
                if (dto.Images.Count > 5)
                    return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT", "Tối đa 5 ảnh mỗi lần gửi."));

                var invalidFormat = dto.Images.FirstOrDefault(f =>
                    !AllowedImageExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()));
                if (invalidFormat != null)
                    return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_FILE_FORMAT",
                        $"Định dạng không hợp lệ: {Path.GetExtension(invalidFormat.FileName)}. Chỉ chấp nhận jpg, jpeg, png."));

                var oversized = dto.Images.FirstOrDefault(f => f.Length > MaxFileSizeBytes);
                if (oversized != null)
                    return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                        ApiResponse<object>.Fail("file too large", "FILE_TOO_LARGE", "Mỗi ảnh tối đa 5MB."));
            }

            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            var result = await _reportService.UpdateReportAsync(userId, id, dto);
            return Ok(ApiResponse<CitizenReportResponseDto>.Ok(result, "report updated successfully"));
        }

        /// <summary>
        /// Citizen hủy báo cáo — chỉ được khi status là Pending.
        /// </summary>
        /// <param name="id">ID của báo cáo</param>
        [Authorize]
        [HttpDelete("/api/v1/users/citizen-reports/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelReport(int id)
        {
            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            await _reportService.CancelReportAsync(userId, id);
            return Ok(ApiResponse<object>.Ok(null!, "report cancelled successfully"));
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
