using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Staff;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Enterprise")]
    [Route("api/v1/staff")]
    public class StaffController : ControllerBase
    {
        private readonly IUserRepository              _userRepository;
        private readonly IEnterpriseStaffRepository   _enterpriseStaffRepository;

        public StaffController(
            IUserRepository             userRepository,
            IEnterpriseStaffRepository  enterpriseStaffRepository)
        {
            _userRepository            = userRepository;
            _enterpriseStaffRepository = enterpriseStaffRepository;
        }

        /// <summary>
        /// Lấy thông tin enterprise mà staff đang đăng nhập được phân công.
        /// </summary>
        [HttpGet("hub")]
        [ProducesResponseType(typeof(ApiResponse<StaffEnterpriseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyHub()
        {
            var email = User.GetEmail();
            if (email is null)
                return Unauthorized(ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return NotFound(ApiResponse<object>.Fail("account not found", "NOT_FOUND"));

            var staff = await _enterpriseStaffRepository.GetByUserIdAsync(user.Id);
            if (staff is null)
                return NotFound(ApiResponse<object>.Fail("staff record not found", "NOT_FOUND"));

            var enterprise = staff.Enterprise;
            var result = new StaffEnterpriseDto
            {
                EnterpriseId      = enterprise.Id,
                EnterpriseName    = enterprise.Name,
                EnterpriseEmail   = enterprise.Email,
                EnterpriseAddress = enterprise.Address,
                WorkAreaId        = enterprise.WorkAreaId,
                WorkAreaName      = enterprise.WorkArea?.Name,
                JoinHubAt         = staff.JoinHubAt
            };

            return Ok(ApiResponse<StaffEnterpriseDto>.Ok(result, "get enterprise info successfully"));
        }
    }
}
