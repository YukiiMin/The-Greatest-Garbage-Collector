using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ITeamSessionRepository
    {
        Task<TeamSession> CreateAsync(TeamSession session);
        Task<TeamSession?> GetByTeamAndDateAsync(Guid teamId, DateOnly date);
        Task UpdateAsync(TeamSession session);
        Task<IReadOnlyList<TeamSession>> GetByTeamSinceAsync(Guid teamId, DateTime since);
        Task<IReadOnlyList<TeamSession>> GetByTeamIdsAsync(IEnumerable<Guid> teamIds, CancellationToken ct = default);
    }
}
