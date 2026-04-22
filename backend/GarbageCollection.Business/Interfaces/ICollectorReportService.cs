using GarbageCollection.Common.DTOs.Collector;
using Microsoft.AspNetCore.Http;

namespace GarbageCollection.Business.Interfaces
{
    public interface ICollectorReportService
    {
        Task<CollectorReportsResponseDto> GetTodayReportsAsync(Guid userId);
        Task<StartShiftResponseDto> StartShiftAsync(Guid userId, int teamId, DateOnly date);
        Task<CollectReportResponseDto> CollectReportAsync(Guid userId, int reportId, List<IFormFile> images);
    }
}
