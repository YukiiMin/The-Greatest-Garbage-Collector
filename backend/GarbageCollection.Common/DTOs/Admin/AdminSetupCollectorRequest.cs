using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class SetupCollectorData
    {
        [Required]
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [JsonPropertyName("enterprise_id")]
        public Guid EnterpriseId { get; set; }
    }

    public class AdminSetupCollectorRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public SetupCollectorData Data { get; set; } = null!;
    }
}
