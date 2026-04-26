namespace GarbageCollection.Common.Models
{
    public class TeamSession
    {
        public Guid      Id            { get; set; } = Guid.NewGuid();
        public Guid      TeamId        { get; set; }
        public DateOnly  Date          { get; set; }
        public DateTime  StartAt       { get; set; }
        public DateTime? EndAt         { get; set; }
        public int       TotalReports  { get; set; }
        public decimal   TotalCapacity { get; set; }
        public DateTime  CreatedAt     { get; set; } = DateTime.UtcNow;

        // Navigation
        public Team Team { get; set; } = null!;
    }
}
