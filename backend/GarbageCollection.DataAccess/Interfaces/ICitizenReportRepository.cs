using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICitizenReportRepository
    {
        Task<CitizenReport> CreateAsync(CitizenReport report);
        Task<CitizenReport?> GetByIdAsync(Guid id);
        Task<CitizenReport?> GetByIdTrackedAsync(Guid id);
        Task<IEnumerable<CitizenReport>> GetByUserIdAsync(Guid userId, ReportStatus? status = null);
        Task<(IEnumerable<CitizenReport> Items, int Total)> GetByUserIdPagedAsync(Guid userId, int page, int limit);
        Task<(IReadOnlyList<CitizenReport> Items, int Total)> GetPagedForEnterpriseAsync(
            IEnumerable<Guid> enterpriseTeamIds,
            IEnumerable<ReportStatus>? statuses,
            int page, int limit, CancellationToken ct = default);
        Task<IReadOnlyList<CitizenReport>> GetAllForEnterpriseAsync(
            IEnumerable<Guid> teamIds, CancellationToken ct = default);
        Task<CitizenReport> UpdateAsync(CitizenReport report);
        Task DeleteAsync(CitizenReport report);
    }
}
