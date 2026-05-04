using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Leaderboard;
using GarbageCollection.Common.Enums;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IUserRepository _userRepository;

        public LeaderboardService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LeaderboardResult> GetLeaderboardAsync(
            Guid userId,
            LeaderboardPeriod period,
            LeaderboardScope scope,
            int page,
            int limit,
            CancellationToken ct = default)
        {
            var me = await _userRepository.GetByIdAsync(userId, ct);

            // scope=Area yêu cầu user đã có work_area_id
            Guid? filterWorkAreaId = null;
            if (scope == LeaderboardScope.Area)
            {
                if (me?.WorkAreaId is null)
                    throw new InvalidOperationException("WORK_AREA_NOT_SET");
                filterWorkAreaId = me.WorkAreaId;
            }

            var myRank  = await _userRepository.GetUserRankAsync(userId, filterWorkAreaId, ct);
            var myScore = me?.TotalPoints ?? 0;

            var (items, total) = await _userRepository.GetLeaderboardPagedAsync(
                filterWorkAreaId, page, limit, ct);

            var entries = items.Select((u, index) => new LeaderboardEntryDto
            {
                Rank        = (page - 1) * limit + index + 1,
                FullName    = u.FullName,
                AvatarUrl   = u.AvatarUrl,
                TotalPoints = u.TotalPoints,
                WorkAreaName = u.WorkArea?.Name
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
