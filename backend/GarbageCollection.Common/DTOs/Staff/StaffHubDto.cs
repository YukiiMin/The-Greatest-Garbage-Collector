using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Staff
{
    public class StaffEnterpriseDto
    {
        [JsonPropertyName("enterprise_id")]
        public Guid EnterpriseId { get; set; }

        [JsonPropertyName("enterprise_name")]
        public string EnterpriseName { get; set; } = string.Empty;

        [JsonPropertyName("enterprise_email")]
        public string EnterpriseEmail { get; set; } = string.Empty;

        [JsonPropertyName("enterprise_address")]
        public string EnterpriseAddress { get; set; } = string.Empty;

        [JsonPropertyName("work_area_id")]
        public Guid? WorkAreaId { get; set; }

        [JsonPropertyName("work_area_name")]
        public string? WorkAreaName { get; set; }

        [JsonPropertyName("join_hub_at")]
        public DateTime? JoinHubAt { get; set; }
    }
}
