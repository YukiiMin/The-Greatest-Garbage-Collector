using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class AdminSetupResponseDto
    {
        [JsonPropertyName("user")]
        public AdminUserDto User { get; set; } = null!;

        [JsonPropertyName("extra_data")]
        public object? ExtraData { get; set; }
    }
}
