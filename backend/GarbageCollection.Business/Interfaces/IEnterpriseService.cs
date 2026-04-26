using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Enterprise;

namespace GarbageCollection.Business.Interfaces
{
    public interface IEnterpriseService
    {
        // ── Dashboard ─────────────────────────────────────────────────────────
        Task<(int, ApiResponse<EnterpriseDashboardData>)> GetDashboardAsync(
            string email, CancellationToken ct = default);

        // ── Reports ───────────────────────────────────────────────────────────
        Task<(int, ApiResponse<EnterpriseReportListResponseDto>)> GetReportsAsync(
            string email, string? status, int page, int limit, CancellationToken ct = default);

        Task<(int, ApiResponse<EnterpriseReportDto>)> GetReportDetailAsync(
            string email, Guid reportId, CancellationToken ct = default);

        Task<(int, ApiResponse<EnterpriseReportDto>)> QueueReportAsync(
            string email, Guid reportId, CancellationToken ct = default);

        Task<(int, ApiResponse<EnterpriseReportDto>)> AssignReportAsync(
            string email, Guid reportId, AssignReportRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<EnterpriseReportDto>)> RejectReportAsync(
            string email, Guid reportId, RejectReportRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<EnterpriseReportDto>)> CompleteReportAsync(
            string email, Guid reportId, CancellationToken ct = default);

        // ── Collectors ────────────────────────────────────────────────────────
        Task<(int, ApiResponse<List<CollectorDto>>)> GetCollectorsAsync(
            string email, CancellationToken ct = default);

        Task<(int, ApiResponse<CollectorDto>)> GetCollectorDetailAsync(
            string email, Guid id, CancellationToken ct = default);

        Task<(int, ApiResponse<CollectorDto>)> CreateCollectorAsync(
            string email, SaveCollectorRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<CollectorDto>)> UpdateCollectorAsync(
            string email, Guid id, SaveCollectorRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<object>)> DeleteCollectorAsync(
            string email, Guid id, CancellationToken ct = default);

        // ── Teams ─────────────────────────────────────────────────────────────
        Task<(int, ApiResponse<List<TeamDetailDto>>)> GetTeamsAsync(
            string email, CancellationToken ct = default);

        Task<(int, ApiResponse<TeamDetailDto>)> GetTeamDetailAsync(
            string email, Guid id, CancellationToken ct = default);

        Task<(int, ApiResponse<TeamDetailDto>)> CreateTeamAsync(
            string email, SaveTeamRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<TeamDetailDto>)> UpdateTeamAsync(
            string email, Guid id, SaveTeamRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<object>)> DeleteTeamAsync(
            string email, Guid id, CancellationToken ct = default);

        // ── Point Categories ──────────────────────────────────────────────────
        Task<(int, ApiResponse<List<PointCategoryDto>>)> GetPointCategoriesAsync(
            string email, CancellationToken ct = default);

        Task<(int, ApiResponse<PointCategoryDto>)> CreatePointCategoryAsync(
            string email, SavePointCategoryRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<PointCategoryDto>)> UpdatePointCategoryAsync(
            string email, Guid id, SavePointCategoryRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<object>)> DeletePointCategoryAsync(
            string email, Guid id, CancellationToken ct = default);

        // ── Staff management ──────────────────────────────────────────────────
        Task<(int, ApiResponse<List<StaffDto>>)> GetTeamStaffAsync(
            string email, Guid teamId, CancellationToken ct = default);

        Task<(int, ApiResponse<StaffDto>)> AddTeamStaffAsync(
            string email, Guid teamId, AddStaffRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<object>)> RemoveTeamStaffAsync(
            string email, Guid teamId, Guid userId, CancellationToken ct = default);
    }
}
