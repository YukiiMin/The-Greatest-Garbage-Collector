using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICollectorReportRepository
    {
        Task<IEnumerable<CitizenReport>> GetActiveByTeamIdAsync(Guid teamId);
        Task<(IEnumerable<CitizenReport> Items, int Total)> GetQueueByTeamIdPagedAsync(Guid teamId, ReportStatus status, int page, int limit);
        Task<int> CountAssignedTodayAsync(Guid teamId, DateOnly date);
        Task<bool> HasProcessingTodayAsync(Guid teamId, DateOnly date);
        Task<int> StartShiftAsync(Guid teamId, DateOnly date);
        Task<CitizenReport> CollectWithPointsAsync(CitizenReport report, List<string> imageUrls, int pointsEarned);
        Task<(int CollectedCount, decimal TotalCapacity)> GetSessionSummaryAsync(Guid teamId, DateOnly date);
        Task<CitizenReport> MarkFailedAsync(CitizenReport report, string reason);
        Task<IEnumerable<CitizenReport>> GetByTeamSinceAsync(Guid teamId, DateTime? since);
    }
}
