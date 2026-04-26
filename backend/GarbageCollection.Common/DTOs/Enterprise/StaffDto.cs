using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class StaffDto
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("user_full_name")]
        public string UserFullName { get; set; } = string.Empty;

        [JsonPropertyName("collector_id")]
        public Guid? CollectorId { get; set; }

        [JsonPropertyName("team_id")]
        public Guid? TeamId { get; set; }

        [JsonPropertyName("join_team_at")]
        public DateTime? JoinTeamAt { get; set; }
    }

    public class AddStaffRequest
    {
        [Required]
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
    }
}
