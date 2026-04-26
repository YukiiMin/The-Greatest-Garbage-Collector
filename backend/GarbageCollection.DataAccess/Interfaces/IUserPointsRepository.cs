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

        Task UpdateWorkAreaNameAsync(Guid userId, string workAreaName, CancellationToken ct = default);

        /// <summary>Reset WeekPoints = 0 cho tất cả users (chạy mỗi thứ Hai).</summary>
        Task ResetWeekPointsAsync(CancellationToken ct = default);

        /// <summary>Reset MonthPoints = 0 cho tất cả users (chạy ngày 1 hàng tháng).</summary>
        Task ResetMonthPointsAsync(CancellationToken ct = default);

        /// <summary>Reset YearPoints = 0 cho tất cả users (chạy ngày 1/1 hàng năm).</summary>
        Task ResetYearPointsAsync(CancellationToken ct = default);
    }
}
