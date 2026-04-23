using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IUserPointsRepository
    {
        Task<UserPoints?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<(IEnumerable<UserPoints> Items, int Total)> GetLeaderboardPagedAsync(
            LeaderboardPeriod period,
            LeaderboardScope scope,
            string? workAreaName,
            int page,
            int limit,
            CancellationToken ct = default);
        Task<int> GetUserRankAsync(
            Guid userId,
            LeaderboardPeriod period,
            LeaderboardScope scope,
            string? workAreaName,
            CancellationToken ct = default);
    }
}
