namespace GarbageCollection.Common.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalCapacity { get; set; }
        public bool IsActive { get; set; } = true;
        public int CollectorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Collector Collector { get; set; } = null!;
    }
}
