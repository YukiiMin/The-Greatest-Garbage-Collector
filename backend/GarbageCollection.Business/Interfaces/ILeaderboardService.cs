using GarbageCollection.Common.DTOs.Leaderboard;
using GarbageCollection.Common.Enums;

namespace GarbageCollection.Business.Interfaces
{
    public interface ILeaderboardService
    {
        Task<LeaderboardResult> GetLeaderboardAsync(
            Guid userId,
            LeaderboardPeriod period,
            LeaderboardScope scope,
            int page,
            int limit,
            CancellationToken ct = default);
    }
}
