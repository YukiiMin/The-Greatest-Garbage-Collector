using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Enterprise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/enterprise")]
    public sealed class EnterpriseController : ControllerBase
    {
        private readonly IEnterpriseService _enterpriseService;

        public EnterpriseController(IEnterpriseService enterpriseService)
        {
            _enterpriseService = enterpriseService;
        }

        // ── Dashboard ─────────────────────────────────────────────────────────

        /// <summary>Dashboard tổng quan của enterprise.</summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<EnterpriseDashboardData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetDashboardAsync(email, ct);
            return StatusCode(statusCode, result);
        }

        // ── Reports ───────────────────────────────────────────────────────────

        /// <summary>Danh sách báo cáo của enterprise, lọc theo status + phân trang.</summary>
        [HttpGet("reports")]
        [ProducesResponseType(typeof(ApiResponse<EnterpriseReportListResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetReports(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken ct = default)
        {
            if (page < 1 || limit < 1 || limit > 50)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid query params", "INVALID_QUERY_PARAMS",
                    "page >= 1, limit must be between 1 and 50"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetReportsAsync(email, status, page, limit, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Chi tiết một báo cáo.</summary>
        [HttpGet("reports/{id}")]
        [ProducesResponseType(typeof(ApiResponse<EnterpriseReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportDetail(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetReportDetailAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Chuyển báo cáo Pending → Queue.</summary>
        [HttpPatch("reports/{id}/queue")]
        [ProducesResponseType(typeof(ApiResponse<EnterpriseReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> QueueReport(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.QueueReportAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Phân công báo cáo Queue → Assigned cho một team.</summary>
        [HttpPatch("reports/{id}/assign")]
        [ProducesResponseType(typeof(ApiResponse<EnterpriseReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AssignReport(
            [FromRoute] Guid id,
            [FromBody] AssignReportRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.AssignReportAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Từ chối báo cáo Pending/Queue → Rejected.</summary>
        [HttpPatch("reports/{id}/reject")]
        [ProducesResponseType(typeof(ApiResponse<EnterpriseReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RejectReport(
            [FromRoute] Guid id,
            [FromBody] RejectReportRequest request,
            CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.RejectReportAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Hoàn thành báo cáo Collected → Completed.</summary>
        [HttpPatch("reports/{id}/complete")]
        [ProducesResponseType(typeof(ApiResponse<EnterpriseReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CompleteReport(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.CompleteReportAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        // ── Collectors ────────────────────────────────────────────────────────

        /// <summary>Danh sách collectors của enterprise.</summary>
        [HttpGet("collectors")]
        [ProducesResponseType(typeof(ApiResponse<List<CollectorDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCollectors(CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetCollectorsAsync(email, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Chi tiết một collector.</summary>
        [HttpGet("collectors/{id}")]
        [ProducesResponseType(typeof(ApiResponse<CollectorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCollectorDetail(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetCollectorDetailAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Tạo collector mới.</summary>
        [HttpPost("collectors")]
        [ProducesResponseType(typeof(ApiResponse<CollectorDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateCollector(
            [FromBody] SaveCollectorRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.CreateCollectorAsync(email, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Cập nhật collector.</summary>
        [HttpPatch("collectors/{id}")]
        [ProducesResponseType(typeof(ApiResponse<CollectorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCollector(
            [FromRoute] Guid id,
            [FromBody] SaveCollectorRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.UpdateCollectorAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Xóa collector (không có teams).</summary>
        [HttpDelete("collectors/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCollector(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.DeleteCollectorAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        // ── Teams ─────────────────────────────────────────────────────────────

        /// <summary>Danh sách tất cả teams thuộc enterprise (qua collectors).</summary>
        [HttpGet("teams")]
        [ProducesResponseType(typeof(ApiResponse<List<TeamDetailDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeams(CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetTeamsAsync(email, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Chi tiết một team.</summary>
        [HttpGet("teams/{id}")]
        [ProducesResponseType(typeof(ApiResponse<TeamDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeamDetail(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetTeamDetailAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Tạo team mới dưới một collector.</summary>
        [HttpPost("teams")]
        [ProducesResponseType(typeof(ApiResponse<TeamDetailDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateTeam(
            [FromBody] SaveTeamRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.CreateTeamAsync(email, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Cập nhật team.</summary>
        [HttpPatch("teams/{id}")]
        [ProducesResponseType(typeof(ApiResponse<TeamDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateTeam(
            [FromRoute] Guid id,
            [FromBody] SaveTeamRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.UpdateTeamAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Xóa team (không có staff).</summary>
        [HttpDelete("teams/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteTeam(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.DeleteTeamAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        // ── Point Categories ──────────────────────────────────────────────────

        /// <summary>Danh sách point categories của enterprise.</summary>
        [HttpGet("point-categories")]
        [ProducesResponseType(typeof(ApiResponse<List<PointCategoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPointCategories(CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetPointCategoriesAsync(email, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Tạo point category mới.</summary>
        [HttpPost("point-categories")]
        [ProducesResponseType(typeof(ApiResponse<PointCategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreatePointCategory(
            [FromBody] SavePointCategoryRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.CreatePointCategoryAsync(email, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Cập nhật point category.</summary>
        [HttpPatch("point-categories/{id}")]
        [ProducesResponseType(typeof(ApiResponse<PointCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePointCategory(
            [FromRoute] Guid id,
            [FromBody] SavePointCategoryRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.UpdatePointCategoryAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Xóa point category.</summary>
        [HttpDelete("point-categories/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePointCategory(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.DeletePointCategoryAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        // ── Staff management ──────────────────────────────────────────────────

        /// <summary>Danh sách staff trong một team.</summary>
        [HttpGet("teams/{teamId}/staff")]
        [ProducesResponseType(typeof(ApiResponse<List<StaffDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeamStaff(
            [FromRoute] Guid teamId, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.GetTeamStaffAsync(email, teamId, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Thêm staff vào team.</summary>
        [HttpPost("teams/{teamId}/staff")]
        [ProducesResponseType(typeof(ApiResponse<StaffDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddTeamStaff(
            [FromRoute] Guid teamId,
            [FromBody] AddStaffRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.AddTeamStaffAsync(email, teamId, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Xóa staff khỏi team.</summary>
        [HttpDelete("teams/{teamId}/staff/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveTeamStaff(
            [FromRoute] Guid teamId,
            [FromRoute] Guid userId,
            CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _enterpriseService.RemoveTeamStaffAsync(email, teamId, userId, ct);
            return StatusCode(statusCode, result);
        }
    }
}
