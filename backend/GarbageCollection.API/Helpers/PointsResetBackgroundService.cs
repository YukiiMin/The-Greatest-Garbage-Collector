namespace GarbageCollection.API.Helpers
{
    /// <summary>
    /// Placeholder — weekly/monthly/yearly point resets were removed when user_points table was merged
    /// into users.total_points. This service no longer does anything but is kept to avoid removing
    /// its registration from Program.cs for now.
    /// </summary>
    public sealed class PointsResetBackgroundService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Task.CompletedTask;
    }
}
