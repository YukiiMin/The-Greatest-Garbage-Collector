namespace GarbageCollection.Common.DTOs.Collector
{
    public class CollectorReportItemDto
    {
        public int Id { get; set; }
        public List<string> WasteCategories { get; set; } = [];
        public decimal? WasteUnit { get; set; }
        public decimal? GpsLat { get; set; }
        public decimal? GpsLng { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];
        public bool PriorityFlag { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? RouteOrder { get; set; }
    }
}
