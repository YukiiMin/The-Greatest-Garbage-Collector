using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class ChangeRoleData
    {
        /// <summary>Citizen | Collector | Enterprise | Admin</summary>
        [Required]
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
    }

    public class ChangeRoleRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public ChangeRoleData Data { get; set; } = new();
    }
}
