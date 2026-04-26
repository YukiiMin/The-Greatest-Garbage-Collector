namespace GarbageCollection.Common.DTOs.Collector
{
    public class TypeCountDto
    {
        public string Type      { get; set; } = string.Empty;
        public int    Collected { get; set; }
    }

    public class TypeCapacityDto
    {
        public string  Type  { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class OverviewDto
    {
        public int              Total     { get; set; }
        public int              Collected { get; set; }
        public int              Failed    { get; set; }
        public List<TypeCountDto> ByType  { get; set; } = [];
    }

    public class CapacityOverviewDto
    {
        public decimal              Total  { get; set; }
        public List<TypeCapacityDto> ByType { get; set; } = [];
    }

    public class MonthlyStatDto
    {
        public string             Month     { get; set; } = string.Empty;
        public int                Total     { get; set; }
        public int                Collected { get; set; }
        public int                Failed    { get; set; }
        public List<TypeCountDto> ByType    { get; set; } = [];
    }

    public class MonthlyCapacityDto
    {
        public string              Month  { get; set; } = string.Empty;
        public decimal             Total  { get; set; }
        public List<TypeCapacityDto> ByType { get; set; } = [];
    }

    public class DailyStatDto
    {
        public string             Date      { get; set; } = string.Empty;
        public int                Total     { get; set; }
        public int                Collected { get; set; }
        public int                Failed    { get; set; }
        public List<TypeCountDto> ByType    { get; set; } = [];
    }

    public class DailyCapacityDto
    {
        public string              Date   { get; set; } = string.Empty;
        public decimal             Total  { get; set; }
        public List<TypeCapacityDto> ByType { get; set; } = [];
    }

    public class SessionItemDto
    {
        public DateTime  StartAt       { get; set; }
        public DateTime? EndAt         { get; set; }
        public int       TotalReports  { get; set; }
        public decimal   TotalCapacity { get; set; }
    }

    public class SessionDayDto
    {
        public string             Date         { get; set; } = string.Empty;
        public int                SessionCount { get; set; }
        public List<SessionItemDto> Sessions   { get; set; } = [];
    }

    public class CollectorDashboardData
    {
        public OverviewDto          Overview         { get; set; } = new();
        public CapacityOverviewDto  CapacityOverview { get; set; } = new();
        public List<MonthlyStatDto>     MonthlyStats     { get; set; } = [];
        public List<MonthlyCapacityDto> MonthlyCapacity  { get; set; } = [];
        public List<DailyStatDto>       DailyStats       { get; set; } = [];
        public List<DailyCapacityDto>   DailyCapacity    { get; set; } = [];
        public List<SessionDayDto>      Sessions         { get; set; } = [];
    }
}
