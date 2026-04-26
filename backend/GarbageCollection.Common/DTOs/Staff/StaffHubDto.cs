using System.Text.Json.Serialization;
using GarbageCollection.Common.DTOs.Enterprise;

namespace GarbageCollection.Common.DTOs.Staff
{
    public class StaffHubDto
    {
        [JsonPropertyName("enterprise_id")]
        public Guid EnterpriseId { get; set; }

        [JsonPropertyName("enterprise_name")]
        public string EnterpriseName { get; set; } = string.Empty;

        [JsonPropertyName("collector_id")]
        public Guid? CollectorId { get; set; }

        /// <summary>Null nếu staff chưa được phân vào hub nào.</summary>
        [JsonPropertyName("hub")]
        public CollectorDto? Hub { get; set; }

        [JsonPropertyName("team_id")]
        public Guid? TeamId { get; set; }

        [JsonPropertyName("join_team_at")]
        public DateTime? JoinTeamAt { get; set; }
    }
}
