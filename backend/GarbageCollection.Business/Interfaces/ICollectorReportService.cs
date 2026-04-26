using GarbageCollection.Common.DTOs.Collector;
using Microsoft.AspNetCore.Http;

namespace GarbageCollection.Business.Interfaces
{
    public interface ICollectorReportService
    {
        Task<CollectorReportsResponseDto> GetTodayReportsAsync(Guid userId);
        Task<StartShiftResponseDto> StartShiftAsync(Guid userId, Guid teamId, DateOnly date);
        Task<CollectReportResponseDto> CollectReportAsync(Guid userId, Guid reportId, List<IFormFile> images);
        Task<CollectReportResponseDto> UpdateReportAsync(Guid userId, Guid reportId, UpdateReportRequest request);
        Task<EndShiftResponseDto> EndShiftAsync(Guid userId, Guid teamId, DateOnly date);
        Task<CollectorDashboardData> GetDashboardAsync(Guid userId);
    }
}
