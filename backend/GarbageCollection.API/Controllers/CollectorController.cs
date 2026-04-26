using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Collector;
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

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public CollectorController(
            ICollectorReportService collectorReportService,
            IUserRepository userRepository)
        {
            _collectorReportService = collectorReportService;
            _userRepository         = userRepository;
        }

        /// <summary>
        /// Lấy danh sách report (Assigned + Processing) của team hôm nay.
        /// </summary>
        [HttpGet("reports")]
        [ProducesResponseType(typeof(ApiResponse<CollectorReportsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTodayReports()
        {
            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var result = await _collectorReportService.GetTodayReportsAsync(user.Id);
            return Ok(ApiResponse<CollectorReportsResponseDto>.Ok(result, "get collector reports successfully"));
        }

        /// <summary>
        /// Bắt đầu ca làm việc — chuyển toàn bộ Assigned → Processing.
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

        /// <summary>
        /// Kết thúc ca làm việc — tổng kết session, set Team.InWork=false.
        /// </summary>
        [HttpPatch("reports/end-shift")]
        [ProducesResponseType(typeof(ApiResponse<EndShiftResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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
        /// Cập nhật kết quả report: Collected (kèm ảnh + actual_capacity_kg) hoặc Failed (kèm lý do).
        /// </summary>
        [HttpPatch("reports/{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CollectReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateReport(Guid id, [FromForm] UpdateReportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "status is required", "MISSING_STATUS"));

            var isCollected = string.Equals(request.Status, "Collected", StringComparison.OrdinalIgnoreCase);
            var isFailed    = string.Equals(request.Status, "Failed",    StringComparison.OrdinalIgnoreCase);

            if (!isCollected && !isFailed)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid status", "INVALID_STATUS", "status must be 'Collected' or 'Failed'"));

            if (isCollected)
            {
                if (request.Images is null || request.Images.Count == 0)
                    return UnprocessableEntity(ApiResponse<object>.Fail(
                        "missing confirmation image", "MISSING_IMAGE",
                        "At least 1 image is required when marking as Collected."));

                var invalidFormat = request.Images.FirstOrDefault(f =>
                    !AllowedImageExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()));
                if (invalidFormat != null)
                    return UnprocessableEntity(ApiResponse<object>.Fail(
                        "invalid file format", "INVALID_FILE_FORMAT",
                        $"Only jpg, jpeg, png are accepted."));

                var oversized = request.Images.FirstOrDefault(f => f.Length > MaxFileSizeBytes);
                if (oversized != null)
                    return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                        ApiResponse<object>.Fail("file too large", "FILE_TOO_LARGE", "Max 5MB per image."));

                if (request.ActualCapacityKg is null or <= 0)
                    return UnprocessableEntity(ApiResponse<object>.Fail(
                        "actual_capacity_kg is required", "MISSING_CAPACITY",
                        "actual_capacity_kg must be > 0 when marking as Collected."));
            }

            if (isFailed && string.IsNullOrWhiteSpace(request.Reason))
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "reason is required", "MISSING_REASON",
                    "reason is required when marking as Failed."));

            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var result = await _collectorReportService.UpdateReportAsync(user.Id, id, request);
            return Ok(ApiResponse<CollectReportResponseDto>.Ok(result, "report updated"));
        }

        /// <summary>
        /// Dashboard thống kê của collector.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<CollectorDashboardData>), StatusCodes.Status200OK)]
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
            return Ok(ApiResponse<CollectorDashboardData>.Ok(result, "dashboard data"));
        }

        /// <summary>
        /// (Legacy) Collector xác nhận đã thu gom — backward compat với /collect.
        /// </summary>
        [HttpPatch("reports/{id:guid}/collect")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CollectReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CollectReport(Guid id, [FromForm] List<IFormFile> images)
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
                    $"Định dạng không hợp lệ: {Path.GetExtension(invalidFormat.FileName)}."));

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
    }
}
