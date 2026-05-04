using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Admin;
using GarbageCollection.Common.DTOs.Complaint;

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

        // ── Create staff account ──────────────────────────────────────────────

        Task<(int, ApiResponse<AdminUserDto>)> CreateStaffAccountAsync(
            string adminEmail, CreateStaffAccountRequest req, CancellationToken ct);

        // ── EnterpriseStaff CRUD (admin-only) ─────────────────────────────────

        Task<(int, ApiResponse<List<AdminEnterpriseStaffDto>>)> GetEnterpriseStaffAsync(
            string adminEmail, Guid enterpriseId, CancellationToken ct);

        Task<(int, ApiResponse<AdminEnterpriseStaffDto>)> AddEnterpriseStaffAsync(
            string adminEmail, Guid enterpriseId, AddEnterpriseStaffRequest req, CancellationToken ct);

        Task<(int, ApiResponse<object>)> RemoveEnterpriseStaffAsync(
            string adminEmail, Guid enterpriseId, Guid userId, CancellationToken ct);
    }
}
