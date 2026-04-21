namespace GarbageCollection.Common.DTOs.WasteReport
{
    public class CitizenReportResponseDto
    {
        public int Id { get; set; }
        public List<string> CitizenImageUrls { get; set; } = [];
        public List<string> Types { get; set; } = [];
        public decimal? Capacity { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public int? PointCategoryId { get; set; }
        public int? Point { get; set; }
        public int? TeamId { get; set; }
        public string? ReportNote { get; set; }
        public DateTime? AssignAt { get; set; }
        public DateTime ReportAt { get; set; }
        public List<string> CollectorImageUrls { get; set; } = [];
        public DateTime? CompleteAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
