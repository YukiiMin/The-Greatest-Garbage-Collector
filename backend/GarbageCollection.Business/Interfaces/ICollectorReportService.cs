using GarbageCollection.Common.DTOs.Collector;
using Microsoft.AspNetCore.Http;

namespace GarbageCollection.Business.Interfaces
{
    public interface ICollectorReportService
    {
        Task<CollectorReportsResponseDto> GetTodayReportsAsync(Guid userId);
        Task<StartShiftResponseDto> StartShiftAsync(Guid userId, Guid teamId, DateOnly date);
        Task<CollectReportResponseDto> CollectReportAsync(Guid userId, Guid reportId, List<IFormFile> images);
    }
}
