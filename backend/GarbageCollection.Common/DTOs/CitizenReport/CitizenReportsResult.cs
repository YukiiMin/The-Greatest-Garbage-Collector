namespace GarbageCollection.Common.DTOs.CitizenReport
{
    public class CitizenReportsResult
    {
        public List<CitizenReportResponseDto> Reports { get; set; } = [];
        public PaginationMeta Pagination { get; set; } = new();
    }
}
