using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(Guid id);
        Task<IEnumerable<Team>> GetByCollectorHubIdAsync(Guid collectorHubId);
        Task<IReadOnlyList<Team>> GetByCollectorHubIdsAsync(IEnumerable<Guid> collectorHubIds);
        Task<IReadOnlyList<Team>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<Team> CreateAsync(Team team);
        Task<Team> UpdateAsync(Team team);
        Task DeleteAsync(Team team);
    }
}
