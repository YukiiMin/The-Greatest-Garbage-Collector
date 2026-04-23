namespace GarbageCollection.Common.DTOs.Collector
{
    public class CollectorReportItemDto
    {
        public Guid Id { get; set; }
        public List<string> WasteCategories { get; set; } = [];
        public decimal? WasteUnit { get; set; }
        public string? UserAddress { get; set; }
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = [];
        public List<string> CollectorImageUrls { get; set; } = [];
        public string Status { get; set; } = string.Empty;
        public string? ReportNote { get; set; }
        public DateTime? AssignAt { get; set; }
        public DateTime ReportAt { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
