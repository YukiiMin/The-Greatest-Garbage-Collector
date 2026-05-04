using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class CreateStaffAccountData
    {
        [Required]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>"Enterprise" hoặc "Collector"</summary>
        [Required]
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
    }

    public class CreateStaffAccountRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public CreateStaffAccountData Data { get; set; } = new();
    }
}
