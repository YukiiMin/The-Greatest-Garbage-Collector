namespace GarbageCollection.Common.Models
{
    /// <summary>Cơ sở vật chất (hub) của một tổ chức Collector cấp Phường.</summary>
    public class CollectorHub
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        /// <summary>ID của WorkArea cấp Ward mà hub đặt tại.</summary>
        public Guid? WorkAreaId { get; set; }

        public int? AssignedCapacity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public WorkArea? WorkArea { get; set; }
    }
}
