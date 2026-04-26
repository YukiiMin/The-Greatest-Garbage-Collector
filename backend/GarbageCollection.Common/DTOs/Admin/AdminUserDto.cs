using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class AdminUserDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("is_banned")]
        public bool IsBanned { get; set; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class AdminUserListResponseDto
    {
        [JsonPropertyName("users")]
        public IReadOnlyList<AdminUserDto> Users { get; set; } = [];

        [JsonPropertyName("pagination")]
        public PaginationMeta Pagination { get; set; } = null!;
    }
}
