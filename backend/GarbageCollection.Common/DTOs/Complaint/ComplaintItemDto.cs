using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public sealed class ComplaintItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("report_id")]
        public int ReportId { get; set; }

        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; } = null!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
