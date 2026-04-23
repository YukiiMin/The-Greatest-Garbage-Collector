using GarbageCollection.Common.Enums;
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

        Task<Complaint?> GetDetailAsync(int complaintId, CancellationToken ct = default);
        Task<IReadOnlyList<Complaint>> GetComplaintsAsync(
          ComplaintStatus status,
          int page,
          int limit,
          CancellationToken ct = default);

        Task<int> CountAsync(
            ComplaintStatus status,
            CancellationToken ct = default);
    }
}
