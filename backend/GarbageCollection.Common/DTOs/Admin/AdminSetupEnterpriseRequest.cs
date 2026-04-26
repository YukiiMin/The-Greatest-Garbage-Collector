using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class SetupEnterpriseData
    {
        [Required]
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("work_area_id")]
        public Guid? WorkAreaId { get; set; }

        [JsonPropertyName("latitude")]
        public decimal? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal? Longitude { get; set; }
    }

    public class AdminSetupEnterpriseRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public SetupEnterpriseData Data { get; set; } = null!;
    }

    public class AssignEnterpriseData
    {
        [Required]
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
    }

    public class AssignEnterpriseRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public AssignEnterpriseData Data { get; set; } = null!;
    }
}
