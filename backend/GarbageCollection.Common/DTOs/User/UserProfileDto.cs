using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.User
{
    public class UserProfileDto
    {
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string Fullname { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public string? Address { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("work_area_id")]
        public Guid? WorkAreaId { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
