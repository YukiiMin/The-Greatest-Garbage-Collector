using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GarbageCollection.DataAccess.Repositories
{
    public class CollectorReportRepository : ICollectorReportRepository
    {
        private readonly AppDbContext _context;

        public CollectorReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<CitizenReport>> GetActiveByTeamIdAsync(int teamId)
        {
            var activeStatuses = new[]
            {
                ReportStatus.Processing,
                ReportStatus.Collected
            };

            return _context.CitizenReports
                .Include(r => r.User)
                .Where(r => r.TeamId == teamId && activeStatuses.Contains(r.Status))
                .OrderBy(r => r.Deadline)
                .ToListAsync()
                .ContinueWith(t => (IEnumerable<CitizenReport>)t.Result);
        }

        public Task<int> CountAssignedTodayAsync(int teamId, DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end   = start.AddDays(1);

            return _context.CitizenReports
                .CountAsync(r =>
                    r.TeamId == teamId &&
                    r.Status == ReportStatus.Assigned &&
                    r.AssignAt >= start && r.AssignAt < end);
        }

        public Task<bool> HasShiftStartedTodayAsync(int teamId, DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end   = start.AddDays(1);

            return _context.CitizenReports
                .AnyAsync(r =>
                    r.TeamId == teamId &&
                    r.Status == ReportStatus.Processing &&
                    r.AssignAt >= start && r.AssignAt < end);
        }

        public async Task<int> StartShiftAsync(int teamId, DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end   = start.AddDays(1);

            return await _context.Database.ExecuteSqlRawAsync(
                @"UPDATE citizen_reports
                  SET status = 'Processing', updated_at = NOW()
                  WHERE team_id = @teamId
                    AND status = 'Assigned'
                    AND assign_at >= @start
                    AND assign_at < @end",
                new NpgsqlParameter("@teamId", teamId),
                new NpgsqlParameter("@start",  start),
                new NpgsqlParameter("@end",    end));
        }

        public async Task<CitizenReport> CollectWithPointsAsync(
            CitizenReport report,
            List<string> imageUrls,
            int pointsEarned)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // B7a: update report
                report.Status             = ReportStatus.Collected;
                report.CollectorImageUrls = imageUrls;
                report.CollectedAt        = DateTime.UtcNow;
                report.UpdatedAt          = DateTime.UtcNow;
                report.Point              = pointsEarned;
                _context.CitizenReports.Update(report);

                // B7b: upsert user_points
                if (pointsEarned > 0)
                {
                    var userPoints = await _context.UserPoints.FindAsync(report.UserId);
                    if (userPoints is null)
                    {
                        _context.UserPoints.Add(new UserPoints
                        {
                            UserId      = report.UserId,
                            TotalPoints = pointsEarned,
                            WeekPoints  = pointsEarned,
                            MonthPoints = pointsEarned,
                            YearPoints  = pointsEarned,
                            UpdatedAt   = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        userPoints.TotalPoints += pointsEarned;
                        userPoints.WeekPoints  += pointsEarned;
                        userPoints.MonthPoints += pointsEarned;
                        userPoints.YearPoints  += pointsEarned;
                        userPoints.UpdatedAt    = DateTime.UtcNow;
                        _context.UserPoints.Update(userPoints);
                    }

                    // B7c: insert point_transaction
                    _context.PointTransactions.Add(new PointTransaction
                    {
                        UserId    = report.UserId,
                        ReportId  = report.Id,
                        Points    = pointsEarned,
                        Type      = "EARN",
                        Reason    = $"Report #{report.Id} collected",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return report;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
