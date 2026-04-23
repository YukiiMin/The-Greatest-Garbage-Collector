using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Collector
{
    public class EndShiftRequest
    {
        [Required]
        public EndShiftRequestData Data { get; set; } = null!;
    }

    public class EndShiftRequestData
    {
        [JsonPropertyName("team_id")]
        [Required]
        public Guid TeamId { get; set; }

        [JsonPropertyName("date")]
        [Required]
        public DateOnly Date { get; set; }
    }
}
