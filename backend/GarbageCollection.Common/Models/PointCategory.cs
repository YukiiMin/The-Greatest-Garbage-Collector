namespace GarbageCollection.Common.Models
{
    public class PointCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public PointMechanic Mechanic { get; set; } = new();
        public int EnterpriseId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Enterprise Enterprise { get; set; } = null!;
    }
}
