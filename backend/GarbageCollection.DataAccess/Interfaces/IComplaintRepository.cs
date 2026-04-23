using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IComplaintRepository
    {
        Task<Complaint> CreateAsync(Complaint complaint);
        Task<Complaint?> GetByIdAsync(Guid id);
        Task<Complaint> UpdateAsync(Complaint complaint);
        Task<(IEnumerable<Complaint> Items, int Total)> GetByReportIdPagedAsync(Guid reportId, int page, int limit);
        Task AppendMessageAsync(Guid complaintId, ComplaintMessage message, CancellationToken ct = default);
    }
}
