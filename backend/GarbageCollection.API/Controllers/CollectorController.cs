using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Collector;
using GarbageCollection.Common.Enums;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/v1/collector")]
    [Authorize]
    public class CollectorController : ControllerBase
    {
        private readonly ICollectorReportService _collectorReportService;
        private readonly IUserRepository _userRepository;

        public CollectorController(
            ICollectorReportService collectorReportService,
            IUserRepository userRepository)
        {
            _collectorReportService = collectorReportService;
            _userRepository         = userRepository;
        }

        /// <summary>
        /// Collector bắt đầu ca làm việc — chuyển toàn bộ QueuedForDispatch → OnTheWay.
        /// </summary>
        [HttpPatch("reports/start-shift")]
        [ProducesResponseType(typeof(ApiResponse<StartShiftResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> StartShift([FromBody] StartShiftRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var result = await _collectorReportService.StartShiftAsync(user.Id, request.Data.TeamId, request.Data.Date);
            return Ok(ApiResponse<StartShiftResponseDto>.Ok(result, "shift started"));
        }

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        /// <summary>
        /// Collector xác nhận đã thu gom — upload ảnh, chuyển trạng thái sang COLLECTED, cộng điểm citizen.
        /// </summary>
        [HttpPatch("reports/{id:guid}/collect")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CollectReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413RequestEntityTooLarge)]
        public async Task<IActionResult> CollectReport(Guid id, [FromForm] List<IFormFile> images)
        {
            // B5: validate images
            if (images is null || images.Count == 0)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "missing confirmation image", "MISSING_IMAGE",
                    "Vui lòng gửi ít nhất 1 ảnh xác nhận."));

            var invalidFormat = images.FirstOrDefault(f =>
                !AllowedImageExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()));
            if (invalidFormat != null)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid file format", "INVALID_FILE_FORMAT",
                    $"Định dạng không hợp lệ: {Path.GetExtension(invalidFormat.FileName)}. Chỉ chấp nhận jpg, jpeg, png."));

            var oversized = images.FirstOrDefault(f => f.Length > MaxFileSizeBytes);
            if (oversized != null)
                return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                    ApiResponse<object>.Fail("file too large", "FILE_TOO_LARGE", "Mỗi ảnh tối đa 5MB."));

            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var result = await _collectorReportService.CollectReportAsync(user.Id, id, images);
            return Ok(ApiResponse<CollectReportResponseDto>.Ok(result, "report collected"));
        }

        /// <summary>
        /// Collector kết thúc ca làm việc — điền EndAt/TotalReports/TotalCapacity vào TeamSession, set team.InWork=false.
        /// </summary>
        [HttpPatch("reports/end-shift")]
        [ProducesResponseType(typeof(ApiResponse<EndShiftResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> EndShift([FromBody] EndShiftRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var result = await _collectorReportService.EndShiftAsync(user.Id, request.Data.TeamId, request.Data.Date);
            return Ok(ApiResponse<EndShiftResponseDto>.Ok(result, "shift ended"));
        }

        /// <summary>
        /// Cập nhật trạng thái report: Collected (upload ảnh) hoặc Failed (lý do).
        /// </summary>
        [HttpPatch("reports/{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CollectReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateReport(
            Guid id,
            [FromForm] string status,
            [FromForm] List<IFormFile>? images,
            [FromForm] string? reason)
        {
            if (status != "Collected" && status != "Failed")
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid status", "INVALID_STATUS",
                    "status must be 'Collected' or 'Failed'"));

            if (status == "Collected")
            {
                if (images is null || images.Count == 0)
                    return UnprocessableEntity(ApiResponse<object>.Fail(
                        "missing confirmation image", "MISSING_IMAGE",
                        "Vui lòng gửi ít nhất 1 ảnh xác nhận."));

                var invalidFormat = images.FirstOrDefault(f =>
                    !AllowedImageExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()));
                if (invalidFormat != null)
                    return UnprocessableEntity(ApiResponse<object>.Fail(
                        "invalid file format", "INVALID_FILE_FORMAT",
                        $"Định dạng không hợp lệ: {Path.GetExtension(invalidFormat.FileName)}. Chỉ chấp nhận jpg, jpeg, png."));

                var oversized = images.FirstOrDefault(f => f.Length > MaxFileSizeBytes);
                if (oversized != null)
                    return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                        ApiResponse<object>.Fail("file too large", "FILE_TOO_LARGE", "Mỗi ảnh tối đa 5MB."));
            }

            if (status == "Failed" && string.IsNullOrWhiteSpace(reason))
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "missing reason", "MISSING_REASON",
                    "Vui lòng cung cấp lý do khi báo thất bại."));

            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var result = await _collectorReportService.UpdateReportAsync(user.Id, id, status, images, reason);
            return Ok(ApiResponse<CollectReportResponseDto>.Ok(result, $"report {status.ToLower()}"));
        }

        /// <summary>
        /// Dashboard thống kê cá nhân collector: overview, capacity, monthly, daily, sessions.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<CollectorDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDashboard()
        {
            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var result = await _collectorReportService.GetDashboardAsync(user.Id);
            return Ok(ApiResponse<CollectorDashboardDto>.Ok(result, "dashboard fetched successfully"));
        }

        /// <summary>
        /// Lấy danh sách report đang chờ xử lý (Assigned + Processing) của team, có phân trang.
        /// </summary>
        [HttpGet("reports")]
        [ProducesResponseType(typeof(ApiResponse<CollectorReportQueueResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetReportQueue(
            [FromQuery] string status = "Assigned",
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (!Enum.TryParse<ReportStatus>(status, ignoreCase: true, out var reportStatus)
                || (reportStatus != ReportStatus.Assigned && reportStatus != ReportStatus.Processing))
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid status", "INVALID_QUERY_PARAMS",
                    "status must be 'Assigned' or 'Processing'"));

            if (page < 1 || limit < 1 || limit > 50)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid query params", "INVALID_QUERY_PARAMS",
                    "page >= 1, limit must be between 1 and 50"));

            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var result = await _collectorReportService.GetReportQueueAsync(user.Id, reportStatus, page, limit);
            return Ok(ApiResponse<CollectorReportQueueResult>.Ok(result, "get collector reports successfully"));
        }
    }
}
