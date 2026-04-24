using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICollectorReportRepository
    {
        Task<IEnumerable<CitizenReport>> GetActiveByTeamIdAsync(Guid teamId);
        Task<int> CountAssignedTodayAsync(Guid teamId, DateOnly date);
        Task<bool> HasOnTheWayTodayAsync(Guid teamId, DateOnly date);
        Task<int> StartShiftAsync(Guid teamId, DateOnly date);
        Task<CitizenReport> CollectWithPointsAsync(CitizenReport report, List<string> imageUrls, int pointsEarned);
    }
}
