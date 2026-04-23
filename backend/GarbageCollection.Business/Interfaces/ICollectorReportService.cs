using GarbageCollection.Common.DTOs.Collector;
using GarbageCollection.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace GarbageCollection.Business.Interfaces
{
    public interface ICollectorReportService
    {
        Task<CollectorReportsResponseDto> GetTodayReportsAsync(Guid userId);
        Task<CollectorReportQueueResult> GetReportQueueAsync(Guid userId, ReportStatus status, int page, int limit);
        Task<StartShiftResponseDto> StartShiftAsync(Guid userId, Guid teamId, DateOnly date);
        Task<CollectReportResponseDto> CollectReportAsync(Guid userId, Guid reportId, List<IFormFile> images);
        Task<EndShiftResponseDto> EndShiftAsync(Guid userId, Guid teamId, DateOnly date);
        Task<CollectReportResponseDto> UpdateReportAsync(Guid userId, Guid reportId, string status, List<IFormFile>? images, string? reason);
        Task<CollectorDashboardDto> GetDashboardAsync(Guid userId);
    }
}
