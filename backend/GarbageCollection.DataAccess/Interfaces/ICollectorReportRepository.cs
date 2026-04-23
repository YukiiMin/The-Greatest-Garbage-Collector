using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICollectorReportRepository
    {
        Task<IEnumerable<CitizenReport>> GetActiveByTeamIdAsync(int teamId);

        /// <summary>
        /// Returns how many Assigned reports exist for the team on the given date.
        /// </summary>
        Task<int> CountAssignedTodayAsync(int teamId, DateOnly date);

        /// <summary>
        /// Returns whether shift was already started today (any Processing report for the team on the given date).
        /// </summary>
        Task<bool> HasShiftStartedTodayAsync(int teamId, DateOnly date);

        /// <summary>
        /// Bulk-updates all QueuedForDispatch reports for the team on the given date to OnTheWay.
        /// Returns the number of rows updated.
        /// </summary>
        Task<int> StartShiftAsync(int teamId, DateOnly date);

        /// <summary>
        /// Atomically marks the report as Collected, stores collector images,
        /// upserts user_points, and inserts a point_transaction — all in one DB transaction.
        /// </summary>
        Task<CitizenReport> CollectWithPointsAsync(
            CitizenReport report,
            List<string> imageUrls,
            int pointsEarned);
    }
}
