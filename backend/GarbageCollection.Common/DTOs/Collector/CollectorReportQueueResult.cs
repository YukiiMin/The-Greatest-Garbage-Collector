using GarbageCollection.Common.DTOs;

namespace GarbageCollection.Common.DTOs.Collector
{
    public class CollectorReportQueueResult
    {
        public Guid TeamId { get; set; }
        public List<CollectorReportItemDto> Reports { get; set; } = [];
        public PaginationMeta Pagination { get; set; } = new();
    }
}
