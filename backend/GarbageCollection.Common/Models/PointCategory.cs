namespace GarbageCollection.Common.Models
{
    public class PointCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public PointMechanic Mechanic { get; set; } = new();
        public Guid EnterpriseId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDelete { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Enterprise Enterprise { get; set; } = null!;
    }
}
