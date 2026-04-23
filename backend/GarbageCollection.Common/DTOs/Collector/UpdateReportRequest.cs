using Microsoft.AspNetCore.Http;

namespace GarbageCollection.Common.DTOs.Collector
{
    public class UpdateReportRequest
    {
        /// <summary>"Collected" or "Failed"</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Required when Status = "Collected"</summary>
        public List<IFormFile>? Images { get; set; }

        /// <summary>Required when Status = "Failed"</summary>
        public string? Reason { get; set; }
    }
}
