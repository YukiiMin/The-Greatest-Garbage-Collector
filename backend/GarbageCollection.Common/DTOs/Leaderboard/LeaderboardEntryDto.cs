namespace GarbageCollection.Common.DTOs.Leaderboard
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int TotalPoints { get; set; }
        public string? WorkAreaName { get; set; }
    }

    public class MyRankDto
    {
        public int Rank { get; set; }
        public int TotalPoints { get; set; }
    }

    public class LeaderboardResult
    {
        public MyRankDto MyRank { get; set; } = new();
        public List<LeaderboardEntryDto> Leaderboard { get; set; } = [];
        public PaginationMeta Pagination { get; set; } = new();
    }
}
