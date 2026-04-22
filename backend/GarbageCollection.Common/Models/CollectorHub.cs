namespace GarbageCollection.Common.Models
{
    public class CollectorHub
    {
        public Guid Id { get; set; }
        public int EnterpriseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public List<Guid> WorkAreaIds { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Enterprise Enterprise { get; set; } = null!;
    }
}
