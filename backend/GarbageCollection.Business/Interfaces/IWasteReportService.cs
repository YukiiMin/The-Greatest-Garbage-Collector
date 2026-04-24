using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.CitizenReport;
using GarbageCollection.Common.Enums;

namespace GarbageCollection.Business.Interfaces
{
    public interface ICitizenReportService
    {
        Task<CitizenReportResponseDto> CreateReportAsync(Guid userId, CreateCitizenReportDto dto);
        Task<CitizenReportResponseDto?> GetReportByIdAsync(Guid id);
        Task<IEnumerable<CitizenReportResponseDto>> GetReportsByUserAsync(Guid userId, ReportStatus? status = null);
        Task<CitizenReportsResult> GetUserReportsPagedAsync(Guid userId, int page, int limit);
        Task CancelReportAsync(Guid userId, Guid reportId);
        Task<CitizenReportResponseDto> UpdateReportAsync(Guid userId, Guid reportId, UpdateCitizenReportDto dto);
        Task<CitizenReportResponseDto> UpdateStatusAsync(Guid reportId, ReportStatus newStatus);
    }
}
