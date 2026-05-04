using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class AdminEnterpriseStaffDto
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("user_full_name")]
        public string UserFullName { get; set; } = string.Empty;

        [JsonPropertyName("enterprise_id")]
        public Guid EnterpriseId { get; set; }

        [JsonPropertyName("join_hub_at")]
        public DateTime? JoinHubAt { get; set; }
    }

    public class AddEnterpriseStaffData
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
    }

    public class AddEnterpriseStaffRequest
    {
        [JsonPropertyName("data")]
        public AddEnterpriseStaffData Data { get; set; } = new();
    }
}
