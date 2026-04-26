using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICollectorReportRepository
    {
        Task<IEnumerable<CitizenReport>> GetActiveByTeamIdAsync(Guid teamId);
        Task<int> CountAssignedTodayAsync(Guid teamId, DateOnly date);
        Task<bool> HasProcessingTodayAsync(Guid teamId, DateOnly date);
        Task<int> StartShiftAsync(Guid teamId, DateOnly date);
        Task<CitizenReport> CollectWithPointsAsync(CitizenReport report, List<string> imageUrls, int pointsEarned, decimal? actualCapacityKg);
        Task MarkFailedAsync(CitizenReport report, string reason);
        Task<(int TotalReports, decimal TotalCapacity)> GetSessionSummaryAsync(Guid teamId, DateOnly date);
        Task<IReadOnlyList<CitizenReport>> GetByTeamSinceAsync(Guid teamId, DateTime since);
    }
}
