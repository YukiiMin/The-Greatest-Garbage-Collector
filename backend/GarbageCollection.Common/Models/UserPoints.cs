namespace GarbageCollection.Common.Models
{
    public class UserPoints
    {
        public Guid UserId { get; set; }
        public int WeekPoints { get; set; }
        public int MonthPoints { get; set; }
        public int YearPoints { get; set; }
        public int TotalPoints { get; set; }
        public bool LeaderboardOptOut { get; set; }
        public string? WorkAreaName { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
    }
}
