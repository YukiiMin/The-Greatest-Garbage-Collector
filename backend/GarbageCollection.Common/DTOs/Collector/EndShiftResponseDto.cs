namespace GarbageCollection.Common.DTOs.Collector
{
    public class EndShiftResponseDto
    {
        public int     TotalReports           { get; set; }
        public decimal TotalCapacity          { get; set; }
        public int     SessionDurationMinutes { get; set; }
    }
}
