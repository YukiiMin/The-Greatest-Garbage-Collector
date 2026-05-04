using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICollectorStaffRepository
    {
        Task<CollectorStaff?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<CollectorStaff>> GetByCollectorIdAsync(Guid collectorId);
        Task<IEnumerable<CollectorStaff>> GetByTeamIdAsync(Guid teamId);
        Task<IEnumerable<CollectorStaff>> GetByCollectorHubIdAsync(Guid collectorHubId);
        Task<CollectorStaff> CreateAsync(CollectorStaff staff);
        Task<CollectorStaff> UpdateAsync(CollectorStaff staff);
        Task DeleteAsync(CollectorStaff staff);
        /// <summary>Gán hoặc bỏ team cho một collector staff.</summary>
        Task SetTeamAsync(Guid userId, Guid? teamId);
    }
}
