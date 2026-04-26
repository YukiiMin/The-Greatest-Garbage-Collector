using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.User
{
    public class UpdateCitizenLocationRequest
    {
        [Required(ErrorMessage = "ward_id is required")]
        [JsonPropertyName("ward_id")]
        public Guid WardId { get; set; }

        [MaxLength(512, ErrorMessage = "address must not exceed 512 characters")]
        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }
}
