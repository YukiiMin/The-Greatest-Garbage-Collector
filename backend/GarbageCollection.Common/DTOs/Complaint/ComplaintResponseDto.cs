using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class ComplaintResponseDto
    {
        public Guid Id { get; set; }
        public Guid CitizenId { get; set; }
        public Guid ReportId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
        public string Status { get; set; } = string.Empty;
        public string? AdminResponse { get; set; }
        public DateTime RequestAt { get; set; }
        public DateTime? ResponseAt { get; set; }
        [JsonPropertyName("complaints")]
        public IReadOnlyList<ComplaintItemDto> Complaints { get; set; } = [];

        [JsonPropertyName("pagination")]
        public PaginationMeta Pagination { get; set; } = null!;
    }
}
