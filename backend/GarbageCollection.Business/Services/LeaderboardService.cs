using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Leaderboard;
using GarbageCollection.Common.Enums;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IUserPointsRepository _userPointsRepository;

        public LeaderboardService(IUserPointsRepository userPointsRepository)
        {
            _userPointsRepository = userPointsRepository;
        }

        public async Task<LeaderboardResult> GetLeaderboardAsync(
            Guid userId,
            LeaderboardPeriod period,
            LeaderboardScope scope,
            int page,
            int limit,
            CancellationToken ct = default)
        {
            // Lấy work area của user hiện tại (dùng cho scope=Area)
            var myPoints = await _userPointsRepository.GetByUserIdAsync(userId, ct);
            var myWorkArea = myPoints?.WorkAreaName;

            // scope=Area yêu cầu user đã chọn phường cư trú
            if (scope == LeaderboardScope.Area && string.IsNullOrEmpty(myWorkArea))
                throw new InvalidOperationException("WORK_AREA_NOT_SET");

            // Lấy rank và điểm của user hiện tại
            var myRank = await _userPointsRepository.GetUserRankAsync(userId, period, scope, myWorkArea, ct);
            var myScore = period switch
            {
                LeaderboardPeriod.Week  => myPoints?.WeekPoints ?? 0,
                LeaderboardPeriod.Month => myPoints?.MonthPoints ?? 0,
                _                       => myPoints?.YearPoints ?? 0
            };

            // Lấy danh sách leaderboard
            var (items, total) = await _userPointsRepository.GetLeaderboardPagedAsync(
                period, scope, myWorkArea, page, limit, ct);

            var entries = items.Select((p, index) => new LeaderboardEntryDto
            {
                Rank         = (page - 1) * limit + index + 1,
                FullName     = p.User.FullName,
                AvatarUrl    = p.User.AvatarUrl,
                TotalPoints  = period switch
                {
                    LeaderboardPeriod.Week  => p.WeekPoints,
                    LeaderboardPeriod.Month => p.MonthPoints,
                    _                       => p.YearPoints
                },
                WorkAreaName = p.WorkAreaName
            }).ToList();

            return new LeaderboardResult
            {
                MyRank = new MyRankDto { Rank = myRank, TotalPoints = myScore },
                Leaderboard = entries,
                Pagination = new PaginationMeta
                {
                    Page       = page,
                    Limit      = limit,
                    Total      = total,
                    TotalPages = (int)Math.Ceiling((double)total / limit)
                }
            };
        }
    }
}
