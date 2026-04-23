using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IComplaintRepository
    {
        Task<Complaint> CreateAsync(Complaint complaint);
        Task<Complaint?> GetByIdAsync(int id);
        Task<Complaint> UpdateAsync(Complaint complaint);
        Task<(IEnumerable<Complaint> Items, int Total)> GetByReportIdPagedAsync(int reportId, int page, int limit);
        Task AppendMessageAsync(int complaintId, ComplaintMessage message, CancellationToken ct = default);
    }
}
