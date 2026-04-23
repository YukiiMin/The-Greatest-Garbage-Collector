using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Collector
{
    public class CollectorDashboardDto
    {
        [JsonPropertyName("overview")]
        public OverviewDto Overview { get; set; } = new();

        [JsonPropertyName("capacity_overview")]
        public CapacityOverviewDto CapacityOverview { get; set; } = new();

        [JsonPropertyName("monthly_stats")]
        public List<MonthlyStatsDto> MonthlyStats { get; set; } = [];

        [JsonPropertyName("monthly_capacity")]
        public List<MonthlyCapacityDto> MonthlyCapacity { get; set; } = [];

        [JsonPropertyName("daily_stats")]
        public List<DailyStatsDto> DailyStats { get; set; } = [];

        [JsonPropertyName("daily_capacity")]
        public List<DailyCapacityDto> DailyCapacity { get; set; } = [];

        [JsonPropertyName("sessions")]
        public List<SessionGroupDto> Sessions { get; set; } = [];
    }

    // ── Overview ──────────────────────────────────────────────────
    public class OverviewDto
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("collected")]
        public int Collected { get; set; }

        [JsonPropertyName("failed")]
        public int Failed { get; set; }

        [JsonPropertyName("by_type")]
        public List<TypeCountDto> ByType { get; set; } = [];
    }

    public class TypeCountDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("collected")]
        public int Collected { get; set; }
    }

    // ── Capacity Overview ─────────────────────────────────────────
    public class CapacityOverviewDto
    {
        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("by_type")]
        public List<TypeCapacityDto> ByType { get; set; } = [];
    }

    public class TypeCapacityDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public decimal Total { get; set; }
    }

    // ── Monthly ───────────────────────────────────────────────────
    public class MonthlyStatsDto
    {
        [JsonPropertyName("month")]
        public string Month { get; set; } = string.Empty; // "2026-04"

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("collected")]
        public int Collected { get; set; }

        [JsonPropertyName("failed")]
        public int Failed { get; set; }

        [JsonPropertyName("by_type")]
        public List<TypeCountDto> ByType { get; set; } = [];
    }

    public class MonthlyCapacityDto
    {
        [JsonPropertyName("month")]
        public string Month { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("by_type")]
        public List<TypeCapacityDto> ByType { get; set; } = [];
    }

    // ── Daily ─────────────────────────────────────────────────────
    public class DailyStatsDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty; // "2026-04-23"

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("collected")]
        public int Collected { get; set; }

        [JsonPropertyName("failed")]
        public int Failed { get; set; }

        [JsonPropertyName("by_type")]
        public List<TypeCountDto> ByType { get; set; } = [];
    }

    public class DailyCapacityDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("by_type")]
        public List<TypeCapacityDto> ByType { get; set; } = [];
    }

    // ── Sessions ──────────────────────────────────────────────────
    public class SessionGroupDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("session_count")]
        public int SessionCount { get; set; }

        [JsonPropertyName("sessions")]
        public List<SessionItemDto> Sessions { get; set; } = [];
    }

    public class SessionItemDto
    {
        [JsonPropertyName("start_at")]
        public DateTime StartAt { get; set; }

        [JsonPropertyName("end_at")]
        public DateTime? EndAt { get; set; }

        [JsonPropertyName("total_reports")]
        public int TotalReports { get; set; }

        [JsonPropertyName("total_capacity")]
        public decimal TotalCapacity { get; set; }
    }
}
