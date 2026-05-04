namespace GarbageCollection.Common.Models
{
    public class PointTransaction
    {
        public Guid    Id          { get; set; } = Guid.NewGuid();
        public Guid    UserId      { get; set; }
        public Guid?   ReportId    { get; set; }
        public int     Points      { get; set; }
        public string  Type        { get; set; } = string.Empty; // "earn" | "spend"
        public string? Description { get; set; }
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

        // Navigation
        public User          User   { get; set; } = null!;
        public CitizenReport? Report { get; set; }
    }
}
