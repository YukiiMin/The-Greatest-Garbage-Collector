using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Collector
{
    public class EndShiftResponseDto
    {
        [JsonPropertyName("updated_count")]
        public int UpdatedCount { get; set; }

        [JsonPropertyName("total_capacity")]
        public decimal TotalCapacity { get; set; }

        [JsonPropertyName("session_duration_minutes")]
        public double SessionDurationMinutes { get; set; }
    }
}
