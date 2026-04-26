using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.API.Helpers
{
    /// <summary>
    /// Background service chạy mỗi đêm lúc 00:00 UTC để reset điểm định kỳ:
    ///   - Thứ Hai  → reset WeekPoints  = 0 cho tất cả users
    ///   - Ngày 1   → reset MonthPoints = 0 cho tất cả users
    ///   - 1/1      → reset YearPoints  = 0 cho tất cả users
    /// </summary>
    public sealed class PointsResetBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<PointsResetBackgroundService> _logger;

        public PointsResetBackgroundService(
            IServiceProvider services,
            ILogger<PointsResetBackgroundService> logger)
        {
            _services = services;
            _logger   = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PointsResetBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Tính thời gian chờ đến 00:00 UTC ngày hôm sau
                var now     = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1); // 00:00 UTC ngày mai
                var delay   = nextRun - now;

                _logger.LogInformation(
                    "PointsReset: next run at {NextRun} UTC (in {Delay:hh\\:mm\\:ss}).",
                    nextRun, delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await RunResetAsync(stoppingToken);
            }

            _logger.LogInformation("PointsResetBackgroundService stopped.");
        }

        private async Task RunResetAsync(CancellationToken ct)
        {
            // Dùng scoped DI vì IUserPointsRepository là Scoped
            await using var scope = _services.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserPointsRepository>();

            var today = DateTime.UtcNow;

            try
            {
                // Thứ Hai → reset tuần
                if (today.DayOfWeek == DayOfWeek.Monday)
                {
                    await repo.ResetWeekPointsAsync(ct);
                    _logger.LogInformation("PointsReset: WeekPoints reset on {Date}.", today.Date);
                }

                // Ngày 1 → reset tháng
                if (today.Day == 1)
                {
                    await repo.ResetMonthPointsAsync(ct);
                    _logger.LogInformation("PointsReset: MonthPoints reset on {Date}.", today.Date);
                }

                // 1 tháng 1 → reset năm
                if (today.Day == 1 && today.Month == 1)
                {
                    await repo.ResetYearPointsAsync(ct);
                    _logger.LogInformation("PointsReset: YearPoints reset on {Date}.", today.Date);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PointsReset: error during reset on {Date}.", today.Date);
            }
        }
    }
}
