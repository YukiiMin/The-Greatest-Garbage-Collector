using GarbageCollection.Common.DTOs.Complaint;

namespace GarbageCollection.Business.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintResponseDto> CreateComplaintAsync(Guid citizenId, Guid reportId, CreateComplaintDto dto);
        Task<ComplaintResponseDto> GetComplaintAsync(Guid citizenId, Guid reportId, Guid complaintId);
        Task<ComplaintsListResult> GetComplaintsByReportAsync(Guid citizenId, Guid reportId, int page, int limit);
        Task SendMessageAsync(Guid citizenId, Guid reportId, Guid complaintId, string message, CancellationToken ct = default);
        Task<ComplaintsListResult> GetUserComplaintsPagedAsync(Guid citizenId, int page, int limit);
    }
}
