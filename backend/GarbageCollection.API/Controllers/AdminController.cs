using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Complaint;
using GarbageCollection.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Authorize]                          // all admin endpoints require a valid JWT
    [Route("api/v1/admin")]
    public sealed class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ── GET /api/v1/admin/complaints ──────────────────────────────────────

        /// <summary>
        /// Returns a paginated list of complaints filtered by status.
        /// Requires the caller to have role = "admin" (checked against the DB).
        /// </summary>
        /// <remarks>
        /// **Query parameters**
        ///
        /// | Parameter | Type   | Default  | Constraints             |
        /// |-----------|--------|----------|-------------------------|
        /// | status    | string | PENDING  | PENDING or RESOLVED     |
        /// | page      | int    | 1        | ≥ 1                     |
        /// | limit     | int    | 10       | 1 – 100                 |
        ///
        /// **Error codes**
        /// - `UNAUTHORIZED` (401) — token missing / invalid / expired (JWT middleware),
        ///   or email in token has no matching DB record
        /// - `FORBIDDEN` (403) — caller is authenticated but does not have admin role
        /// - `INVALID_QUERY` (422) — status not recognised, page &lt; 1, or limit out of range
        /// </remarks>
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
            // ── Extract the validated email claim (HTTP concern) ───────────────
            // [Authorize] guarantees the JWT is valid before this action runs.
            // The service uses this email to look up the user's role in the DB.
            var tokenEmail = User.GetEmail();
            if (tokenEmail is null)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    "unauthorized",
                    "UNAUTHORIZED",
                    "Access token does not contain a valid email claim."));
            }

            // ── Build the request DTO from query params ────────────────────────
            // No validation here — the service owns all input validation.
            var request = new GetComplaintsRequestDto
            {
                Status = status,
                Page = page,
                Limit = limit
            };

            // ── Delegate ALL business logic to the service ────────────────────
            var result = await _adminService.GetComplaintsAsync(tokenEmail, request, ct);

            if (!result.Succeeded)
            {
                return StatusCode(
                    result.HttpStatusCode,
                    ApiResponse<object>.Fail(
                        result.FailMessage!,
                        result.FailCode!,
                        result.FailDescription!));
            }

            return Ok(ApiResponse<ComplaintResponseDto>.Success(
                "success",
                result.Payload!));
        }
       
    }
}
