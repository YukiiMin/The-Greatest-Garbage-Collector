using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class ResolveComplaintData
    {
        /// <summary>Approved or Rejected</summary>
        [Required]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Admin's response message sent back to the citizen.</summary>
        [Required]
        [MaxLength(2000)]
        [JsonPropertyName("admin_response")]
        public string AdminResponse { get; set; } = string.Empty;
    }

    public class ResolveComplaintRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public ResolveComplaintData Data { get; set; } = new();
    }
}
