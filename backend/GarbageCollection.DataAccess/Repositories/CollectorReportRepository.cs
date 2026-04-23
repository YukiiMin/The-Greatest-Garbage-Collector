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

        public Task<IEnumerable<CitizenReport>> GetActiveByTeamIdAsync(Guid teamId)
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

        public async Task<(IEnumerable<CitizenReport> Items, int Total)> GetQueueByTeamIdPagedAsync(Guid teamId, ReportStatus status, int page, int limit)
        {
            var query = _context.CitizenReports
                .Include(r => r.User)
                .Where(r => r.TeamId == teamId && r.Status == status)
                .OrderBy(r => r.Deadline);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (items, total);
        }

        public Task<int> CountAssignedTodayAsync(Guid teamId, DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end   = start.AddDays(1);

            return _context.CitizenReports
                .CountAsync(r =>
                    r.TeamId == teamId &&
                    r.Status == ReportStatus.Assigned &&
                    r.AssignAt >= start && r.AssignAt < end);
        }

        public Task<bool> HasProcessingTodayAsync(Guid teamId, DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end   = start.AddDays(1);

            return _context.CitizenReports
                .AnyAsync(r =>
                    r.TeamId == teamId &&
                    r.Status == ReportStatus.Processing &&
                    r.AssignAt >= start && r.AssignAt < end);
        }

        public async Task<int> StartShiftAsync(Guid teamId, DateOnly date)
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

        public async Task<(int CollectedCount, decimal TotalCapacity)> GetSessionSummaryAsync(Guid teamId, DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end   = start.AddDays(1);

            var reports = await _context.CitizenReports
                .Where(r =>
                    r.TeamId == teamId &&
                    r.Status == ReportStatus.Collected &&
                    r.AssignAt >= start && r.AssignAt < end)
                .ToListAsync();

            var totalCapacity = reports.Sum(r => r.Capacity ?? 0m);
            return (reports.Count, totalCapacity);
        }

        public async Task<CitizenReport> MarkFailedAsync(CitizenReport report, string reason)
        {
            report.Status     = ReportStatus.Failed;
            report.ReportNote = reason;
            report.UpdatedAt  = DateTime.UtcNow;
            _context.CitizenReports.Update(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public Task<IEnumerable<CitizenReport>> GetByTeamSinceAsync(Guid teamId, DateTime? since)
        {
            var query = _context.CitizenReports
                .Where(r => r.TeamId == teamId);

            if (since.HasValue)
                query = query.Where(r => r.ReportAt >= since.Value);

            return query
                .OrderByDescending(r => r.ReportAt)
                .ToListAsync()
                .ContinueWith(t => (IEnumerable<CitizenReport>)t.Result);
        }

        public async Task<CitizenReport> CollectWithPointsAsync(
            CitizenReport report,
            List<string> imageUrls,
            int pointsEarned)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                report.Status             = ReportStatus.Collected;
                report.CollectorImageUrls = imageUrls;
                report.CollectedAt        = DateTime.UtcNow;
                report.UpdatedAt          = DateTime.UtcNow;
                report.Point              = pointsEarned;
                _context.CitizenReports.Update(report);

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
