namespace GarbageCollection.Common.DTOs.Collector
{
    public class CollectorReportsResponseDto
    {
        public Guid TeamId { get; set; }
        public Guid? WorkAreaId { get; set; }
        public string? DispatchTime { get; set; }
        public List<CollectorReportItemDto> Reports { get; set; } = [];
        public bool RouteOptimized { get; set; }
    }
}
