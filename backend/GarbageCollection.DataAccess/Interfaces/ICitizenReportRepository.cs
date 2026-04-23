using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICitizenReportRepository
    {
        Task<CitizenReport> CreateAsync(CitizenReport report);
        Task<CitizenReport?> GetByIdAsync(Guid id);
        Task<IEnumerable<CitizenReport>> GetByUserIdAsync(Guid userId, ReportStatus? status = null);
        Task<(IEnumerable<CitizenReport> Items, int Total)> GetByUserIdPagedAsync(Guid userId, int page, int limit);
        Task<CitizenReport> UpdateAsync(CitizenReport report);
        Task DeleteAsync(CitizenReport report);
    }
}
