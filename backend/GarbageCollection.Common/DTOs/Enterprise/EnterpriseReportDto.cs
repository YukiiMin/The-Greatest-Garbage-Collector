namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class EnterpriseReportDto
    {
        public Guid Id { get; set; }
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }
        public List<string> Types { get; set; } = [];
        public decimal? Capacity { get; set; }
        public decimal? ActualCapacityKg { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CitizenEmail { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ReportNote { get; set; }
        public List<string> CitizenImageUrls { get; set; } = [];
        public List<string> CollectorImageUrls { get; set; } = [];
        public DateTime ReportAt { get; set; }
        public DateTime? AssignAt { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? CollectedAt { get; set; }
        public DateTime? CompleteAt { get; set; }
    }

    public class EnterpriseReportListResponseDto
    {
        public List<EnterpriseReportDto> Reports { get; set; } = [];
        public PaginationMeta Pagination { get; set; } = new();
    }
}
