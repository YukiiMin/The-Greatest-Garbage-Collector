using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class CollectorDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public decimal? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal? Longitude { get; set; }

        [JsonPropertyName("work_area_id")]
        public Guid? WorkAreaId { get; set; }

        [JsonPropertyName("work_area_name")]
        public string? WorkAreaName { get; set; }

        [JsonPropertyName("assigned_capacity")]
        public int? AssignedCapacity { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class SaveCollectorData
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public decimal? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal? Longitude { get; set; }

        [JsonPropertyName("work_area_id")]
        public Guid? WorkAreaId { get; set; }

        [JsonPropertyName("assigned_capacity")]
        public int? AssignedCapacity { get; set; }
    }

    public class SaveCollectorRequest
    {
        [Required]
        public SaveCollectorData Data { get; set; } = null!;
    }
}
