namespace GarbageCollection.Common.Models
{
    public class PointTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public int ReportId { get; set; }
        public int Points { get; set; }

        /// <summary>EARN | REFUND | PENALTY</summary>
        public string Type { get; set; } = string.Empty;

        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public CitizenReport Report { get; set; } = null!;
    }
}
