using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IComplaintRepository
    {
        Task<Complaint> CreateAsync(Complaint complaint);
        Task<Complaint> UpdateAsync(Complaint complaint);
        Task<Complaint?> GetByIdAsync(Guid complaintId, CancellationToken ct = default);
        Task<(IEnumerable<Complaint> Items, int Total)> GetByReportIdPagedAsync(Guid reportId, int page, int limit);
        Task AppendMessageAsync(Guid complaintId, ComplaintMessage message, CancellationToken ct = default);
        Task<Complaint?> GetByIdWithReportAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
        Task<IReadOnlyList<Complaint>> GetComplaintsAsync(
          ComplaintStatus status,
          int page,
          int limit,
          CancellationToken ct = default);

        Task<int> CountAsync(
            ComplaintStatus status,
            CancellationToken ct = default);

        Task ResolveAsync(
    Guid complaintId,
    string adminResponse,
    Guid adminId,
    DateTime resolvedAt,
    CancellationToken ct = default);


    }
}
