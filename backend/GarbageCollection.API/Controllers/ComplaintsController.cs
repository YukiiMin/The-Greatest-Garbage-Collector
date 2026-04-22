using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Complaint;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly IUserRepository _userRepository;

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public ComplaintsController(IComplaintService complaintService, IUserRepository userRepository)
        {
            _complaintService = complaintService;
            _userRepository   = userRepository;
        }

        /// <summary>
        /// Citizen tạo complaint cho một báo cáo rác.
        /// </summary>
        /// <param name="reportId">ID của báo cáo</param>
        /// <param name="dto">Lý do và ảnh đính kèm (jpg/png/jpeg, tối đa 5MB/ảnh)</param>
        [Authorize]
        [HttpPost("/api/v1/users/citizen-reports/{reportId:int}/complaints")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ComplaintResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413RequestEntityTooLarge)]
        public async Task<IActionResult> CreateComplaint(int reportId, [FromForm] CreateComplaintDto dto)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT"));

            if (dto.Images.Count > 0)
            {
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

            var result = await _complaintService.CreateComplaintAsync(userId, reportId, dto);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<ComplaintResponseDto>.Ok(result, "complaint created successfully"));
        }

        /// <summary>
        /// Lấy danh sách complaints theo reportId, hỗ trợ phân trang.
        /// </summary>
        /// <param name="reportId">ID của báo cáo</param>
        /// <param name="page">Trang hiện tại (mặc định 1)</param>
        /// <param name="limit">Số item mỗi trang (1–50, mặc định 10)</param>
        [Authorize]
        [HttpGet("/api/v1/users/citizen-reports/{reportId:int}/complaints")]
        [ProducesResponseType(typeof(ApiResponse<ComplaintsListResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetComplaints(int reportId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            if (page < 1 || limit < 1 || limit > 50)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid query params", "INVALID_QUERY_PARAMS"));

            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            var result = await _complaintService.GetComplaintsByReportAsync(userId, reportId, page, limit);
            return Ok(ApiResponse<ComplaintsListResult>.Ok(result, "get complaints successfully"));
        }

        /// <summary>
        /// Lấy thông tin chi tiết một complaint.
        /// </summary>
        /// <param name="reportId">ID của báo cáo</param>
        /// <param name="id">ID của complaint</param>
        [Authorize]
        [HttpGet("/api/v1/users/citizen-reports/{reportId:int}/complaints/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ComplaintResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetComplaint(int reportId, int id)
        {
            var (userId, authErr) = await GetAuthorizedUserAsync();
            if (authErr is not null) return authErr;

            var result = await _complaintService.GetComplaintAsync(userId, reportId, id);
            return Ok(ApiResponse<ComplaintResponseDto>.Ok(result, "get complaint successfully"));
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
