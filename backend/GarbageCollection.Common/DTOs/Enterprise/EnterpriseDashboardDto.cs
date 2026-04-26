using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class EnterpriseTodayDto
    {
        [JsonPropertyName("pending")]     public int Pending     { get; set; }
        [JsonPropertyName("queue")]       public int Queue       { get; set; }
        [JsonPropertyName("assigned")]    public int Assigned    { get; set; }
        [JsonPropertyName("processing")]  public int Processing  { get; set; }
        [JsonPropertyName("collected")]   public int Collected   { get; set; }
        [JsonPropertyName("completed")]   public int Completed   { get; set; }
        [JsonPropertyName("failed")]      public int Failed      { get; set; }
        [JsonPropertyName("rejected")]    public int Rejected    { get; set; }
        [JsonPropertyName("active_teams")] public int ActiveTeams { get; set; }
    }

    public class EnterpriseSummaryDto
    {
        [JsonPropertyName("total")]               public int     Total              { get; set; }
        [JsonPropertyName("completed")]           public int     Completed          { get; set; }
        [JsonPropertyName("failed")]              public int     Failed             { get; set; }
        [JsonPropertyName("rejected")]            public int     Rejected           { get; set; }
        [JsonPropertyName("completion_rate")]     public decimal CompletionRate     { get; set; }
        [JsonPropertyName("avg_processing_hours")] public double AvgProcessingHours { get; set; }
        [JsonPropertyName("total_kg")]            public decimal TotalKg            { get; set; }
    }

    public class EnterpriseTypeCapacityDto
    {
        [JsonPropertyName("type")]     public string  Type    { get; set; } = string.Empty;
        [JsonPropertyName("total_kg")] public decimal TotalKg { get; set; }
    }

    public class EnterpriseCapacityDto
    {
        [JsonPropertyName("total_kg")] public decimal TotalKg { get; set; }
        [JsonPropertyName("by_type")]  public List<EnterpriseTypeCapacityDto> ByType { get; set; } = [];
    }

    public class EnterpriseMonthlyDto
    {
        [JsonPropertyName("month")]     public string  Month     { get; set; } = string.Empty;
        [JsonPropertyName("total")]     public int     Total     { get; set; }
        [JsonPropertyName("completed")] public int     Completed { get; set; }
        [JsonPropertyName("failed")]    public int     Failed    { get; set; }
        [JsonPropertyName("rejected")]  public int     Rejected  { get; set; }
        [JsonPropertyName("total_kg")]  public decimal TotalKg   { get; set; }
    }

    public class EnterpriseTeamPerformanceDto
    {
        [JsonPropertyName("team_id")]       public Guid    TeamId        { get; set; }
        [JsonPropertyName("team_name")]     public string  TeamName      { get; set; } = string.Empty;
        [JsonPropertyName("collector_name")] public string CollectorName { get; set; } = string.Empty;
        [JsonPropertyName("total")]         public int     Total         { get; set; }
        [JsonPropertyName("completed")]     public int     Completed     { get; set; }
        [JsonPropertyName("failed")]        public int     Failed        { get; set; }
        [JsonPropertyName("total_kg")]      public decimal TotalKg       { get; set; }
        [JsonPropertyName("session_count")] public int     SessionCount  { get; set; }
    }

    public class EnterpriseDashboardData
    {
        [JsonPropertyName("today")]   public EnterpriseTodayDto                Today   { get; set; } = new();
        [JsonPropertyName("summary")] public EnterpriseSummaryDto              Summary { get; set; } = new();
        [JsonPropertyName("capacity")] public EnterpriseCapacityDto            Capacity { get; set; } = new();
        [JsonPropertyName("monthly")] public List<EnterpriseMonthlyDto>        Monthly { get; set; } = [];
        [JsonPropertyName("teams")]   public List<EnterpriseTeamPerformanceDto> Teams  { get; set; } = [];
    }
}
