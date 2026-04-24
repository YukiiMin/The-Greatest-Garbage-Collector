namespace GarbageCollection.Common.Models
{
    public class Enterprise
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>Địa chỉ nơi đặt trung tâm tái chế (text + toạ độ).</summary>
        public string Address { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        /// <summary>Khu vực hoạt động của trung tâm tái chế (mô tả hoặc GeoJSON polygon).</summary>
        public string WorkArea { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
