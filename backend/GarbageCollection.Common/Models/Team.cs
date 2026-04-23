namespace GarbageCollection.Common.Models
{
    public class Team
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public decimal TotalCapacity { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CollectorId { get; set; }
        public Guid? WorkAreaId { get; set; }
        public string? DispatchTime { get; set; } // e.g. "20:00"
        public bool RouteOptimized { get; set; } = false;
        public bool InWork { get; set; } = false;
        public DateTime? StartWorkingTime { get; set; }
        public DateTime? LastFinishTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Collector Collector { get; set; } = null!;
    }
}
