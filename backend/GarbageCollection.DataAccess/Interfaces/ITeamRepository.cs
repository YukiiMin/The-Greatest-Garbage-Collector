using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(int id);
        Task<IEnumerable<Team>> GetByCollectorIdAsync(int collectorId);
        Task<Team> CreateAsync(Team team);
        Task<Team> UpdateAsync(Team team);
        Task DeleteAsync(Team team);
    }
}
