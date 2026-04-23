using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ITeamSessionRepository
    {
        Task<TeamSession> CreateAsync(TeamSession session);
        Task<TeamSession?> GetActiveByTeamIdAsync(Guid teamId, DateOnly date);
        Task<TeamSession> UpdateAsync(TeamSession session);
        Task<IEnumerable<TeamSession>> GetByTeamIdAsync(Guid teamId);
    }
}
