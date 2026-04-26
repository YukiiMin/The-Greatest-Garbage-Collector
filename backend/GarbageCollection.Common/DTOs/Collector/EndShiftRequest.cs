using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Collector
{
    public class EndShiftData
    {
        [JsonPropertyName("team_id")]
        public Guid TeamId { get; set; }

        [JsonPropertyName("date")]
        public DateOnly Date { get; set; }
    }

    public class EndShiftRequest
    {
        [JsonPropertyName("data")]
        public EndShiftData Data { get; set; } = new();
    }
}
