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
        /// Lấy danh sách report cần thu gom hôm nay của collector.
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
    }
}
