namespace GarbageCollection.Common.DTOs.Collector
{
    public class CollectReportResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> CollectorImageUrls { get; set; } = [];
        public DateTime? CollectedAt { get; set; }
    }
}
