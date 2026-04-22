using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class UserPointsRepository : IUserPointsRepository
    {
        private readonly AppDbContext _context;

        public UserPointsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserPoints?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
            => await _context.UserPoints
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        public async Task<(IEnumerable<UserPoints> Items, int Total)> GetLeaderboardPagedAsync(
            LeaderboardPeriod period,
            LeaderboardScope scope,
            string? workAreaName,
            int page,
            int limit,
            CancellationToken ct = default)
        {
            var query = _context.UserPoints
                .Include(p => p.User)
                .Where(p => !p.LeaderboardOptOut);

            if (scope == LeaderboardScope.Area && workAreaName != null)
                query = query.Where(p => p.WorkAreaName == workAreaName);

            query = period switch
            {
                LeaderboardPeriod.Week  => query.OrderByDescending(p => p.WeekPoints),
                LeaderboardPeriod.Month => query.OrderByDescending(p => p.MonthPoints),
                _                       => query.OrderByDescending(p => p.YearPoints)
            };

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<int> GetUserRankAsync(
            Guid userId,
            LeaderboardPeriod period,
            LeaderboardScope scope,
            string? workAreaName,
            CancellationToken ct = default)
        {
            var myPoints = await _context.UserPoints
                .FirstOrDefaultAsync(p => p.UserId == userId, ct);

            if (myPoints is null) return 0;

            var myScore = period switch
            {
                LeaderboardPeriod.Week  => myPoints.WeekPoints,
                LeaderboardPeriod.Month => myPoints.MonthPoints,
                _                       => myPoints.YearPoints
            };

            var query = _context.UserPoints
                .Where(p => !p.LeaderboardOptOut);

            if (scope == LeaderboardScope.Area && workAreaName != null)
                query = query.Where(p => p.WorkAreaName == workAreaName);

            var higherCount = period switch
            {
                LeaderboardPeriod.Week  => await query.CountAsync(p => p.WeekPoints > myScore, ct),
                LeaderboardPeriod.Month => await query.CountAsync(p => p.MonthPoints > myScore, ct),
                _                       => await query.CountAsync(p => p.YearPoints > myScore, ct)
            };

            return higherCount + 1;
        }
    }
}
