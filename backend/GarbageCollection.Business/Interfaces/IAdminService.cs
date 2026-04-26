using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Admin;
using GarbageCollection.Common.DTOs.Complaint;
using GarbageCollection.Common.Enums;

namespace GarbageCollection.Business.Interfaces
{
    public interface IAdminService
    {
        Task<GetComplaintsResult> GetComplaintsAsync(
            string tokenEmail,
            GetComplaintsRequestDto request,
            CancellationToken ct = default);

        Task<(int, ApiResponse<ComplaintDetailResponseDto>)> GetComplaintDetailAsync(
            string email,
            Guid complaintId,
            CancellationToken ct = default);

        Task<(int, ApiResponse<ComplaintDetailResponseDto>)> ResolveComplaintAsync(
            string email,
            Guid complaintId,
            ResolveComplaintRequest request,
            CancellationToken ct = default);

        Task<(int, ApiResponse<AdminUserListResponseDto>)> GetUsersAsync(
            string email,
            string? search,
            string? role,
            bool? isBanned,
            int page,
            int limit,
            CancellationToken ct = default);

        Task<(int, ApiResponse<AdminUserDto>)> ChangeRoleAsync(
            string email,
            Guid targetUserId,
            ChangeRoleRequest request,
            CancellationToken ct = default);

        Task<(int, ApiResponse<AdminUserDto>)> BanUserAsync(
            string email,
            Guid targetUserId,
            BanUserRequest request,
            CancellationToken ct = default);

        // ── Enterprise CRUD ───────────────────────────────────────────────────

        Task<(int, ApiResponse<List<AdminEnterpriseDto>>)> GetEnterprisesAsync(
            string adminEmail, CancellationToken ct);

        Task<(int, ApiResponse<AdminEnterpriseDto>)> GetEnterpriseDetailAsync(
            string adminEmail, Guid id, CancellationToken ct);

        Task<(int, ApiResponse<AdminEnterpriseDto>)> CreateEnterpriseAsync(
            string adminEmail, SaveAdminEnterpriseRequest req, CancellationToken ct);

        Task<(int, ApiResponse<AdminEnterpriseDto>)> UpdateEnterpriseAsync(
            string adminEmail, Guid id, SaveAdminEnterpriseRequest req, CancellationToken ct);

        Task<(int, ApiResponse<object>)> DeleteEnterpriseAsync(
            string adminEmail, Guid id, CancellationToken ct);

        // ── Setup accounts ────────────────────────────────────────────────────

        // Bước 1: tạo enterprise hub (chưa link user)
        Task<(int, ApiResponse<AdminEnterpriseDto>)> SetupEnterpriseUserAsync(
            string adminEmail, AdminSetupEnterpriseRequest req, CancellationToken ct);

        // Bước 2: gán enterprise cho user (tạo Staff + đổi role)
        Task<(int, ApiResponse<AdminSetupResponseDto>)> AssignEnterpriseUserAsync(
            string adminEmail, Guid enterpriseId, AssignEnterpriseRequest req, CancellationToken ct);

        Task<(int, ApiResponse<AdminSetupResponseDto>)> SetupCollectorUserAsync(
            string adminEmail, AdminSetupCollectorRequest req, CancellationToken ct);
    }
}
