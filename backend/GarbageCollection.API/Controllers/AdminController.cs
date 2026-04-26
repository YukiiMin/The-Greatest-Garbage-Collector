using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Admin;
using GarbageCollection.Common.DTOs.Complaint;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/admin")]
    public sealed class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IWorkAreaService _workAreaService;

        public AdminController(IAdminService adminService, IWorkAreaService workAreaService)
        {
            _adminService     = adminService;
            _workAreaService  = workAreaService;
        }

        // ── Complaints ────────────────────────────────────────────────────────

        /// <summary>Danh sách khiếu nại phân trang, lọc theo status.</summary>
        [HttpGet("complaints")]
        [ProducesResponseType(typeof(ApiResponse<ComplaintResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetComplaints(
            [FromQuery] string status = "PENDING",
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken ct = default)
        {
            if (page < 1 || limit < 1 || limit > 50)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid query params", "INVALID_QUERY_PARAMS",
                    "page >= 1, limit must be between 1 and 50"));

            var tokenEmail = User.GetEmail();
            if (tokenEmail is null)
                return Unauthorized(ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED",
                    "Access token does not contain a valid email claim."));

            var result = await _adminService.GetComplaintsAsync(tokenEmail,
                new GetComplaintsRequestDto { Status = status, Page = page, Limit = limit }, ct);

            if (!result.Succeeded)
                return StatusCode(result.HttpStatusCode,
                    ApiResponse<object>.Fail(result.FailMessage!, result.FailCode!, result.FailDescription!));

            return Ok(ApiResponse<ComplaintResponseDto>.Success("success", result.Payload!));
        }

        /// <summary>Chi tiết một khiếu nại.</summary>
        [HttpGet("complaints/{id}")]
        public async Task<IActionResult> GetComplaintDetail(
            [FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.GetComplaintDetailAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Admin duyệt hoặc từ chối khiếu nại.</summary>
        [HttpPatch("complaints/{id}")]
        [ProducesResponseType(typeof(ApiResponse<ComplaintDetailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ResolveComplaint(
            [FromRoute] Guid id,
            [FromBody] ResolveComplaintRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.ResolveComplaintAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        // ── Users ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Danh sách user phân trang với filter.
        /// Query: ?search=&amp;role=Citizen|Collector|Enterprise|Admin&amp;is_banned=true|false&amp;page=1&amp;limit=10
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(ApiResponse<AdminUserListResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? is_banned = null,
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

            var (statusCode, result) = await _adminService.GetUsersAsync(
                email, search, role, is_banned, page, limit, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>
        /// Đổi role của user.
        /// Body: { "data": { "role": "Citizen|Collector|Enterprise|Admin" } }
        /// </summary>
        [HttpPatch("users/{id}/role")]
        [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ChangeRole(
            [FromRoute] Guid id,
            [FromBody] ChangeRoleRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.ChangeRoleAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>
        /// Ban hoặc unban user.
        /// Body: { "data": { "is_banned": true|false } }
        /// </summary>
        [HttpPatch("users/{id}/ban")]
        [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> BanUser(
            [FromRoute] Guid id,
            [FromBody] BanUserRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.BanUserAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        // ── Enterprise CRUD ───────────────────────────────────────────────────

        /// <summary>Danh sách tất cả enterprises.</summary>
        [HttpGet("enterprises")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminEnterpriseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEnterprises(CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.GetEnterprisesAsync(email, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Chi tiết một enterprise.</summary>
        [HttpGet("enterprises/{id}")]
        [ProducesResponseType(typeof(ApiResponse<AdminEnterpriseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEnterpriseDetail([FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.GetEnterpriseDetailAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Tạo enterprise mới.</summary>
        [HttpPost("enterprises")]
        [ProducesResponseType(typeof(ApiResponse<AdminEnterpriseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateEnterprise(
            [FromBody] SaveAdminEnterpriseRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.CreateEnterpriseAsync(email, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Cập nhật enterprise.</summary>
        [HttpPatch("enterprises/{id}")]
        [ProducesResponseType(typeof(ApiResponse<AdminEnterpriseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEnterprise(
            [FromRoute] Guid id,
            [FromBody] SaveAdminEnterpriseRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.UpdateEnterpriseAsync(email, id, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Xóa enterprise (không có staff).</summary>
        [HttpDelete("enterprises/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteEnterprise([FromRoute] Guid id, CancellationToken ct)
        {
            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.DeleteEnterpriseAsync(email, id, ct);
            return StatusCode(statusCode, result);
        }

        // ── Setup accounts ────────────────────────────────────────────────────

        /// <summary>Bước 1: Tạo enterprise hub (chỉ tạo Enterprise record, chưa link user).</summary>
        [HttpPost("setup/enterprise")]
        [ProducesResponseType(typeof(ApiResponse<AdminEnterpriseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SetupEnterpriseUser(
            [FromBody] AdminSetupEnterpriseRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.SetupEnterpriseUserAsync(email, request, ct);
            return StatusCode(statusCode, result);
        }

        /// <summary>Bước 2: Gán enterprise cho user (tạo Staff record + đổi role → Enterprise).</summary>
        [HttpPost("setup/enterprise/{enterpriseId}/assign")]
        [ProducesResponseType(typeof(ApiResponse<AdminSetupResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignEnterpriseUser(
            [FromRoute] Guid enterpriseId,
            [FromBody] AssignEnterpriseRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.AssignEnterpriseUserAsync(email, enterpriseId, request, ct);
            return StatusCode(statusCode, result);
        }

        // ── WorkArea CRUD ─────────────────────────────────────────────────────

        /// <summary>Danh sách work areas. Query: ?type=District|Ward</summary>
        [HttpGet("work-areas")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkAreaDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWorkAreas([FromQuery] string? type = null)
        {
            try
            {
                var result = await _workAreaService.GetAllAsync(type);
                return Ok(ApiResponse<List<WorkAreaDto>>.Success("success", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message, "INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        /// <summary>Chi tiết một work area kèm children.</summary>
        [HttpGet("work-areas/{id}")]
        [ProducesResponseType(typeof(ApiResponse<WorkAreaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkAreaDetail([FromRoute] Guid id)
        {
            try
            {
                var result = await _workAreaService.GetByIdAsync(id);
                return Ok(ApiResponse<WorkAreaDto>.Success("success", result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message, "NOT_FOUND", ex.Message));
            }
        }

        /// <summary>Tạo District (không cần parent_id).</summary>
        [HttpPost("work-areas/districts")]
        [ProducesResponseType(typeof(ApiResponse<WorkAreaDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDistrict([FromBody] CreateDistrictRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            try
            {
                var result = await _workAreaService.CreateDistrictAsync(request);
                return StatusCode(201, ApiResponse<WorkAreaDto>.Success("district created", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message, "BAD_REQUEST", ex.Message));
            }
        }

        /// <summary>Tạo Ward (parent_id bắt buộc — phải là District).</summary>
        [HttpPost("work-areas/wards")]
        [ProducesResponseType(typeof(ApiResponse<WorkAreaDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateWard([FromBody] CreateWardRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            try
            {
                var result = await _workAreaService.CreateWardAsync(request);
                return StatusCode(201, ApiResponse<WorkAreaDto>.Success("ward created", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message, "BAD_REQUEST", ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message, "NOT_FOUND", ex.Message));
            }
        }

        /// <summary>Cập nhật work area.</summary>
        [HttpPatch("work-areas/{id}")]
        [ProducesResponseType(typeof(ApiResponse<WorkAreaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateWorkArea([FromRoute] Guid id, [FromBody] SaveWorkAreaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            try
            {
                var result = await _workAreaService.UpdateAsync(id, request);
                return Ok(ApiResponse<WorkAreaDto>.Success("work area updated", result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message, "NOT_FOUND", ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message, "BAD_REQUEST", ex.Message));
            }
        }

        /// <summary>Xóa work area (nếu không có entity FK vào).</summary>
        [HttpDelete("work-areas/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteWorkArea([FromRoute] Guid id)
        {
            try
            {
                await _workAreaService.DeleteAsync(id);
                return Ok(ApiResponse<object>.Success("work area deleted", null!));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message, "NOT_FOUND", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.Fail(ex.Message, "CONFLICT", ex.Message));
            }
        }

        /// <summary>Tạo tài khoản collector: tạo Staff record + đổi role.</summary>
        [HttpPost("setup/collector")]
        [ProducesResponseType(typeof(ApiResponse<AdminSetupResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SetupCollectorUser(
            [FromBody] AdminSetupCollectorRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("invalid input", "INVALID_INPUT"));

            var email = User.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED", "Invalid token"));

            var (statusCode, result) = await _adminService.SetupCollectorUserAsync(email, request, ct);
            return StatusCode(statusCode, result);
        }
    }
}
