using GarbageCollection.Common.DTOs.Complaint;

namespace GarbageCollection.Business.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintResponseDto> CreateComplaintAsync(Guid citizenId, int reportId, CreateComplaintDto dto);
        Task<ComplaintResponseDto> GetComplaintAsync(Guid citizenId, int reportId, int complaintId);
        Task<ComplaintsListResult> GetComplaintsByReportAsync(Guid citizenId, int reportId, int page, int limit);
    }
}
