using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class BanUserData
    {
        [Required]
        [JsonPropertyName("is_banned")]
        public bool IsBanned { get; set; }
    }

    public class BanUserRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public BanUserData Data { get; set; } = new();
    }
}
