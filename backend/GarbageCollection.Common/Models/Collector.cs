namespace GarbageCollection.Common.Models
{
    /// <summary>Tổ chức thu gom cấp Phường, thuộc quản lý của một Enterprise cấp Quận.</summary>
    public class Collector
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        /// <summary>ID của WorkArea cấp Ward mà collector phụ trách.</summary>
        public Guid? WorkAreaId { get; set; }

        /// <summary>FK → Enterprise (1 Enterprise quản lý nhiều Collector).</summary>
        public Guid EnterpriseId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Enterprise Enterprise { get; set; } = null!;
        public WorkArea? WorkArea { get; set; }
    }
}
