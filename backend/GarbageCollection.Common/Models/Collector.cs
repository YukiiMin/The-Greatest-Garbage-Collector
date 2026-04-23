namespace GarbageCollection.Common.Models
{
    public class Collector
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>Địa chỉ nơi đặt trung tâm thu gom (text + toạ độ).</summary>
        public string Address { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        /// <summary>Khu vực hoạt động đi thu gom rác (mô tả hoặc GeoJSON polygon).</summary>
        public string WorkArea { get; set; } = string.Empty;

        public int? AssignedCapacity { get; set; }
        public int EnterpriseId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Enterprise Enterprise { get; set; } = null!;
    }
}
