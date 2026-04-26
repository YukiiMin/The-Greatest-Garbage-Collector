using Microsoft.AspNetCore.Http;

namespace GarbageCollection.Common.DTOs.Collector
{
    public class UpdateReportRequest
    {
        public string Status { get; set; } = string.Empty;           // "Collected" | "Failed"
        public List<IFormFile>? Images { get; set; }                 // required when Collected
        public string? Reason { get; set; }                          // required when Failed
        public decimal? ActualCapacityKg { get; set; }              // required when Collected
    }
}
