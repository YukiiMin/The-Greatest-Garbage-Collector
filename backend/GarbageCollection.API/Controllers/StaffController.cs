using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Enterprise;
using GarbageCollection.Common.DTOs.Staff;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/staff")]
    public class StaffController : ControllerBase
    {
        private readonly IUserRepository       _userRepository;
        private readonly IStaffRepository      _staffRepository;
        private readonly ICollectorRepository  _collectorRepository;

        public StaffController(
            IUserRepository      userRepository,
            IStaffRepository     staffRepository,
            ICollectorRepository collectorRepository)
        {
            _userRepository      = userRepository;
            _staffRepository     = staffRepository;
            _collectorRepository = collectorRepository;
        }

        /// <summary>
        /// Lấy thông tin enterprise hub đang được phân cho staff đang đăng nhập.
        /// Hub là null nếu staff chưa được gán vào hub nào.
        /// </summary>
        [HttpGet("hub")]
        [ProducesResponseType(typeof(ApiResponse<StaffHubDto>), StatusCodes.Status200OK)]
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

            var staff = await _staffRepository.GetByUserIdAsync(user.Id);
            if (staff is null)
                return NotFound(ApiResponse<object>.Fail("staff record not found", "NOT_FOUND"));

            CollectorDto? hubDto = null;
            if (staff.CollectorId.HasValue)
            {
                var collector = await _collectorRepository.GetByIdAsync(staff.CollectorId.Value);
                if (collector is not null)
                {
                    hubDto = new CollectorDto
                    {
                        Id               = collector.Id,
                        Name             = collector.Name,
                        PhoneNumber      = collector.PhoneNumber,
                        Email            = collector.Email,
                        Address          = collector.Address,
                        Latitude         = collector.Latitude,
                        Longitude        = collector.Longitude,
                        WorkAreaId       = collector.WorkAreaId,
                        WorkAreaName     = collector.WorkArea?.Name,
                        AssignedCapacity = collector.AssignedCapacity,
                        CreatedAt        = collector.CreatedAt,
                        UpdatedAt        = collector.UpdatedAt
                    };
                }
            }

            var result = new StaffHubDto
            {
                EnterpriseId   = staff.EnterpriseId,
                EnterpriseName = staff.Enterprise.Name,
                CollectorId    = staff.CollectorId,
                Hub            = hubDto,
                TeamId         = staff.TeamId,
                JoinTeamAt     = staff.JoinTeamAt
            };

            return Ok(ApiResponse<StaffHubDto>.Ok(result, "get hub info successfully"));
        }
    }
}
