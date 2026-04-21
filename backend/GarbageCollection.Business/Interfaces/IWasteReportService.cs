using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.WasteReport;
using GarbageCollection.Common.Enums;

namespace GarbageCollection.Business.Interfaces
{
    public interface ICitizenReportService
    {
        Task<CitizenReportResponseDto> CreateReportAsync(Guid userId, CreateCitizenReportDto dto);
        Task<CitizenReportResponseDto?> GetReportByIdAsync(int id);
        Task<IEnumerable<CitizenReportResponseDto>> GetReportsByUserAsync(Guid userId, ReportStatus? status = null);
        Task<CitizenReportsResult> GetUserReportsPagedAsync(Guid userId, int page, int limit);
        Task CancelReportAsync(Guid userId, int reportId);
        Task<CitizenReportResponseDto> UpdateReportAsync(Guid userId, int reportId, UpdateCitizenReportDto dto);
        Task<CitizenReportResponseDto> UpdateStatusAsync(int reportId, ReportStatus newStatus);
    }
}
