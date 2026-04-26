using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.User
{
    public class UpdateUserProfileRequest
    {
        [MaxLength(256, ErrorMessage = "fullname must not exceed 256 characters")]
        [JsonPropertyName("fullname")]
        public string? Fullname { get; set; }
    }
}
