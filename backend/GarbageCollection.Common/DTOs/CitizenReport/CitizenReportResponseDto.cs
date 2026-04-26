namespace GarbageCollection.Common.DTOs.CitizenReport
{
    public class CitizenReportResponseDto
    {
        public Guid Id { get; set; }
        public List<string> CitizenImageUrls { get; set; } = [];
        public List<string> Types { get; set; } = [];
        public decimal? Capacity { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid? PointCategoryId { get; set; }
        public int? Point { get; set; }
        public Guid? TeamId { get; set; }
        public string? ReportNote { get; set; }
        public Guid? AssignBy { get; set; }
        public DateTime? AssignAt { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartCollectingAt { get; set; }
        public DateTime? CollectedAt { get; set; }
        public DateTime ReportAt { get; set; }
        public List<string> CollectorImageUrls { get; set; } = [];
        public DateTime? CompleteAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Thông tin vị trí của người tạo báo cáo
        public Guid? CitizenWorkAreaId { get; set; }
        public string? CitizenAddress { get; set; }
    }
}
