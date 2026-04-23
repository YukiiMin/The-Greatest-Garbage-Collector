namespace GarbageCollection.Common.DTOs.Collector
{
    public class CollectorReportItemDto
    {
        public int Id { get; set; }
        public List<string> WasteCategories { get; set; } = [];
        public decimal? WasteUnit { get; set; }
        public string? UserAddress { get; set; }
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];
        public string Status { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
    }
}
