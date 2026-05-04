using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Collector;

namespace GarbageCollection.Business.Interfaces
{
    public interface ICollectorService
    {
        // ── Staff ─────────────────────────────────────────────────────────────
        Task<(int, ApiResponse<List<CollectorStaffDto>>)> GetStaffAsync(
            string email, CancellationToken ct = default);

        Task<(int, ApiResponse<CollectorStaffDto>)> AddStaffAsync(
            string email, AddCollectorStaffRequest request, CancellationToken ct = default);

        Task<(int, ApiResponse<object>)> RemoveStaffAsync(
            string email, Guid userId, CancellationToken ct = default);
    }
}
