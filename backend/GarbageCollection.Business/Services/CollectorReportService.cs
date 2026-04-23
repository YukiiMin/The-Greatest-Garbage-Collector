using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs.Collector;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GarbageCollection.Business.Services
{
    public class CollectorReportService : ICollectorReportService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ICollectorReportRepository _reportRepository;
        private readonly ICitizenReportRepository _citizenReportRepository;
        private readonly IPointCategoryRepository _pointCategoryRepository;
        private readonly IUploadImageService _uploadImageService;
        private readonly ITeamSessionRepository _sessionRepository;

        public CollectorReportService(
            IStaffRepository staffRepository,
            ITeamRepository teamRepository,
            ICollectorReportRepository reportRepository,
            ICitizenReportRepository citizenReportRepository,
            IPointCategoryRepository pointCategoryRepository,
            IUploadImageService uploadImageService,
            ITeamSessionRepository sessionRepository)
        {
            _staffRepository           = staffRepository;
            _teamRepository            = teamRepository;
            _reportRepository          = reportRepository;
            _citizenReportRepository   = citizenReportRepository;
            _pointCategoryRepository   = pointCategoryRepository;
            _uploadImageService        = uploadImageService;
            _sessionRepository         = sessionRepository;
        }

        public async Task<CollectorReportsResponseDto> GetTodayReportsAsync(Guid userId)
        {
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Staff record not found for this user.");

            var team = await _teamRepository.GetByIdAsync(staff.TeamId)
                ?? throw new KeyNotFoundException($"Team {staff.TeamId} not found.");

            var reports = await _reportRepository.GetActiveByTeamIdAsync(team.Id);

            return new CollectorReportsResponseDto
            {
                TeamId         = team.Id,
                WorkAreaId     = team.WorkAreaId,
                DispatchTime   = team.DispatchTime,
                RouteOptimized = team.RouteOptimized,
                Reports        = reports.Select(MapToItem).ToList()
            };
        }

        public async Task<StartShiftResponseDto> StartShiftAsync(Guid userId, Guid teamId, DateOnly date)
        {
            // B1: verify staff belongs to the requested team
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Staff record not found for this user.");

            if (staff.TeamId != teamId)
                throw new UnauthorizedAccessException("you do not belong to this team");

            var team = await _teamRepository.GetByIdAsync(teamId)
                ?? throw new KeyNotFoundException($"Team {teamId} not found.");

            // B2: check server time ≥ dispatch_time (default 20:00)
            var dispatchHour   = 20;
            var dispatchMinute = 0;
            if (!string.IsNullOrWhiteSpace(team.DispatchTime))
            {
                var parts = team.DispatchTime.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out var h) && int.TryParse(parts[1], out var m))
                {
                    dispatchHour   = h;
                    dispatchMinute = m;
                }
            }

            var nowUtc        = DateTime.UtcNow;
            var dispatchUtc   = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, dispatchHour, dispatchMinute, 0, DateTimeKind.Utc);
            if (nowUtc < dispatchUtc)
                throw new InvalidOperationException($"too early — shift starts at {team.DispatchTime ?? "20:00"}");

            // B3: check reports exist
            var queued = await _reportRepository.CountAssignedTodayAsync(teamId, date);
            if (queued == 0)
                throw new KeyNotFoundException("no reports assigned for dispatch today");

            // B4: check shift not already started
            if (await _reportRepository.HasProcessingTodayAsync(teamId, date))
                throw new InvalidOperationException("shift already started today");

            // B5: bulk update Assigned → Processing
            var updated = await _reportRepository.StartShiftAsync(teamId, date);

            // B6: create TeamSession record
            var now = DateTime.UtcNow;
            await _sessionRepository.CreateAsync(new TeamSession
            {
                TeamId    = teamId,
                Date      = date,
                StartAt   = now,
                CreatedAt = now
            });

            // B7: mark team as InWork
            team.InWork = true;
            await _teamRepository.UpdateAsync(team);

            return new StartShiftResponseDto
            {
                UpdatedCount = updated,
                AssignTime   = team.DispatchTime
            };
        }

        public async Task<CollectReportResponseDto> CollectReportAsync(Guid userId, Guid reportId, List<IFormFile> images)
        {
            // B2: find report
            var report = await _citizenReportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException($"report {reportId} not found");

            // B3: check team ownership
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new UnauthorizedAccessException("staff record not found");

            if (report.TeamId != staff.TeamId)
                throw new UnauthorizedAccessException("you are not in the team assigned to this report");

            // B4: status must be Processing
            if (report.Status != ReportStatus.Processing)
                throw new InvalidOperationException($"cannot collect report with status '{report.Status}'");

            // B6: upload images
            var imageUrls = await _uploadImageService.UploadImagesAsync(images, "collector-reports");

            // calculate points from PointCategory if set
            var points = 0;
            if (report.PointCategoryId.HasValue)
            {
                var category = await _pointCategoryRepository.GetByIdAsync(report.PointCategoryId.Value);
                if (category is not null)
                    points = CalculatePoints(report.Types, category.Mechanic);
            }

            // B7: atomic transaction
            var updated = await _reportRepository.CollectWithPointsAsync(report, imageUrls, points);

            return new CollectReportResponseDto
            {
                Id                 = updated.Id,
                Status             = updated.Status.ToString(),
                CollectorImageUrls = updated.CollectorImageUrls,
                CollectedAt        = updated.CollectedAt
            };
        }

        private static int CalculatePoints(List<WasteType> types, PointMechanic mechanic)
        {
            var total = 0m;
            foreach (var type in types)
            {
                total += type switch
                {
                    WasteType.Organic       => mechanic.Organic.Points,
                    WasteType.Recyclable    => mechanic.Recyclable.Points,
                    WasteType.NonRecyclable => mechanic.NonRecyclable.Points,
                    _                       => 0
                };
            }
            return (int)Math.Round(total);
        }

        public async Task<CollectorReportQueueResult> GetReportQueueAsync(Guid userId, ReportStatus status, int page, int limit)
        {
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Staff record not found for this user.");

            var (items, total) = await _reportRepository.GetQueueByTeamIdPagedAsync(staff.TeamId, status, page, limit);

            return new CollectorReportQueueResult
            {
                TeamId     = staff.TeamId,
                Reports    = items.Select(MapToItem).ToList(),
                Pagination = new GarbageCollection.Common.DTOs.PaginationMeta
                {
                    Page       = page,
                    Limit      = limit,
                    Total      = total,
                    TotalPages = (int)Math.Ceiling((double)total / limit)
                }
            };
        }

        // ── END SHIFT ────────────────────────────────────────────────────────────
        public async Task<EndShiftResponseDto> EndShiftAsync(Guid userId, Guid teamId, DateOnly date)
        {
            // B1: verify staff belongs to team
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Staff record not found for this user.");

            if (staff.TeamId != teamId)
                throw new UnauthorizedAccessException("you do not belong to this team");

            var team = await _teamRepository.GetByIdAsync(teamId)
                ?? throw new KeyNotFoundException($"Team {teamId} not found.");

            // B2: team must be InWork
            if (!team.InWork)
                throw new InvalidOperationException("shift has not started or already ended");

            // B3: check no reports still in Processing
            if (await _reportRepository.HasProcessingTodayAsync(teamId, date))
                throw new InvalidOperationException("there are still reports being processed — finish them first");

            // B4: get session summary
            var (collectedCount, totalCapacity) = await _reportRepository.GetSessionSummaryAsync(teamId, date);

            // B5: update TeamSession
            var session = await _sessionRepository.GetActiveByTeamIdAsync(teamId, date)
                ?? throw new InvalidOperationException("no active session found for this team today");

            session.EndAt         = DateTime.UtcNow;
            session.TotalReports  = collectedCount;
            session.TotalCapacity = totalCapacity;
            await _sessionRepository.UpdateAsync(session);

            // B6: mark team as done
            team.InWork         = false;
            team.LastFinishTime = DateTime.UtcNow;
            await _teamRepository.UpdateAsync(team);

            var durationMinutes = (session.EndAt.Value - session.StartAt).TotalMinutes;

            return new EndShiftResponseDto
            {
                UpdatedCount           = collectedCount,
                TotalCapacity          = totalCapacity,
                SessionDurationMinutes = Math.Round(durationMinutes, 2)
            };
        }

        // ── UPDATE REPORT (Collected or Failed) ───────────────────────────────
        public async Task<CollectReportResponseDto> UpdateReportAsync(
            Guid userId, Guid reportId, string status,
            List<IFormFile>? images, string? reason)
        {
            var report = await _citizenReportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException($"Report {reportId} not found.");

            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new UnauthorizedAccessException("staff record not found");

            if (report.TeamId != staff.TeamId)
                throw new UnauthorizedAccessException("you are not in the team assigned to this report");

            if (report.Status != ReportStatus.Processing)
                throw new InvalidOperationException($"cannot update report with status '{report.Status}'");

            if (status == "Collected")
            {
                if (images is null || images.Count == 0)
                    throw new ArgumentException("images are required when status is Collected");

                var imageUrls = await _uploadImageService.UploadImagesAsync(images, "collector-reports");

                var points = 0;
                if (report.PointCategoryId.HasValue)
                {
                    var category = await _pointCategoryRepository.GetByIdAsync(report.PointCategoryId.Value);
                    if (category is not null)
                        points = CalculatePoints(report.Types, category.Mechanic);
                }

                var updated = await _reportRepository.CollectWithPointsAsync(report, imageUrls, points);
                return new CollectReportResponseDto
                {
                    Id                 = updated.Id,
                    Status             = updated.Status.ToString(),
                    CollectorImageUrls = updated.CollectorImageUrls,
                    CollectedAt        = updated.CollectedAt
                };
            }
            else // Failed
            {
                if (string.IsNullOrWhiteSpace(reason))
                    throw new ArgumentException("reason is required when status is Failed");

                var updated = await _reportRepository.MarkFailedAsync(report, reason);
                return new CollectReportResponseDto
                {
                    Id                 = updated.Id,
                    Status             = updated.Status.ToString(),
                    CollectorImageUrls = updated.CollectorImageUrls,
                    CollectedAt        = updated.CollectedAt
                };
            }
        }

        // ── DASHBOARD ─────────────────────────────────────────────────────────
        public async Task<CollectorDashboardDto> GetDashboardAsync(Guid userId)
        {
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Staff record not found for this user.");

            var reports = (await _reportRepository.GetByTeamSinceAsync(staff.TeamId, staff.JoinTeamAt)).ToList();
            var sessions = (await _sessionRepository.GetByTeamIdAsync(staff.TeamId)).ToList();

            var collectedReports = reports.Where(r => r.Status == ReportStatus.Collected).ToList();
            var failedReports    = reports.Where(r => r.Status == ReportStatus.Failed).ToList();

            // ── Overview ──────────────────────────────────────────
            var allWasteTypes = Enum.GetValues<WasteType>();

            var overview = new OverviewDto
            {
                Total     = reports.Count,
                Collected = collectedReports.Count,
                Failed    = failedReports.Count,
                ByType    = allWasteTypes
                    .Select(wt => new TypeCountDto
                    {
                        Type      = wt.ToString(),
                        Collected = collectedReports.Count(r => r.Types.Contains(wt))
                    })
                    .Where(x => x.Collected > 0)
                    .ToList()
            };

            // ── Capacity Overview ──────────────────────────────────
            var capacityOverview = new CapacityOverviewDto
            {
                Total  = collectedReports.Sum(r => r.Capacity ?? 0m),
                ByType = allWasteTypes
                    .Select(wt => new TypeCapacityDto
                    {
                        Type  = wt.ToString(),
                        Total = collectedReports
                            .Where(r => r.Types.Contains(wt))
                            .Sum(r => r.Capacity ?? 0m)
                    })
                    .Where(x => x.Total > 0)
                    .ToList()
            };

            // ── Monthly stats ──────────────────────────────────────
            var monthlyGroups = reports
                .GroupBy(r => r.ReportAt.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .ToList();

            var monthlyStats = monthlyGroups.Select(g =>
            {
                var col = g.Where(r => r.Status == ReportStatus.Collected).ToList();
                var fail = g.Count(r => r.Status == ReportStatus.Failed);
                return new MonthlyStatsDto
                {
                    Month     = g.Key,
                    Total     = g.Count(),
                    Collected = col.Count,
                    Failed    = fail,
                    ByType    = allWasteTypes
                        .Select(wt => new TypeCountDto { Type = wt.ToString(), Collected = col.Count(r => r.Types.Contains(wt)) })
                        .Where(x => x.Collected > 0)
                        .ToList()
                };
            }).ToList();

            var monthlyCapacity = monthlyGroups.Select(g =>
            {
                var col = g.Where(r => r.Status == ReportStatus.Collected).ToList();
                return new MonthlyCapacityDto
                {
                    Month  = g.Key,
                    Total  = col.Sum(r => r.Capacity ?? 0m),
                    ByType = allWasteTypes
                        .Select(wt => new TypeCapacityDto { Type = wt.ToString(), Total = col.Where(r => r.Types.Contains(wt)).Sum(r => r.Capacity ?? 0m) })
                        .Where(x => x.Total > 0)
                        .ToList()
                };
            }).ToList();

            // ── Daily stats ────────────────────────────────────────
            var dailyGroups = reports
                .GroupBy(r => r.ReportAt.ToString("yyyy-MM-dd"))
                .OrderByDescending(g => g.Key)
                .Take(30)
                .ToList();

            var dailyStats = dailyGroups.Select(g =>
            {
                var col  = g.Where(r => r.Status == ReportStatus.Collected).ToList();
                var fail = g.Count(r => r.Status == ReportStatus.Failed);
                return new DailyStatsDto
                {
                    Date      = g.Key,
                    Total     = g.Count(),
                    Collected = col.Count,
                    Failed    = fail,
                    ByType    = allWasteTypes
                        .Select(wt => new TypeCountDto { Type = wt.ToString(), Collected = col.Count(r => r.Types.Contains(wt)) })
                        .Where(x => x.Collected > 0)
                        .ToList()
                };
            }).OrderBy(d => d.Date).ToList();

            var dailyCapacity = dailyGroups.Select(g =>
            {
                var col = g.Where(r => r.Status == ReportStatus.Collected).ToList();
                return new DailyCapacityDto
                {
                    Date   = g.Key,
                    Total  = col.Sum(r => r.Capacity ?? 0m),
                    ByType = allWasteTypes
                        .Select(wt => new TypeCapacityDto { Type = wt.ToString(), Total = col.Where(r => r.Types.Contains(wt)).Sum(r => r.Capacity ?? 0m) })
                        .Where(x => x.Total > 0)
                        .ToList()
                };
            }).OrderBy(d => d.Date).ToList();

            // ── Sessions ───────────────────────────────────────────
            var sessionGroups = sessions
                .GroupBy(s => s.Date.ToString("yyyy-MM-dd"))
                .OrderByDescending(g => g.Key)
                .Select(g => new SessionGroupDto
                {
                    Date         = g.Key,
                    SessionCount = g.Count(),
                    Sessions     = g.OrderBy(s => s.StartAt).Select(s => new SessionItemDto
                    {
                        StartAt        = s.StartAt,
                        EndAt          = s.EndAt,
                        TotalReports   = s.TotalReports,
                        TotalCapacity  = s.TotalCapacity
                    }).ToList()
                })
                .ToList();

            return new CollectorDashboardDto
            {
                Overview         = overview,
                CapacityOverview = capacityOverview,
                MonthlyStats     = monthlyStats,
                MonthlyCapacity  = monthlyCapacity,
                DailyStats       = dailyStats,
                DailyCapacity    = dailyCapacity,
                Sessions         = sessionGroups
            };
        }

        private static CollectorReportItemDto MapToItem(CitizenReport r) => new()
        {
            Id                 = r.Id,
            WasteCategories    = r.Types.Select(t => t.ToString()).ToList(),
            WasteUnit          = r.Capacity,
            UserAddress        = r.User?.Address,
            Description        = r.Description,
            ImageUrls          = r.CitizenImageUrls,
            CollectorImageUrls = r.CollectorImageUrls,
            Status             = r.Status.ToString(),
            ReportNote         = r.ReportNote,
            AssignAt           = r.AssignAt,
            ReportAt           = r.ReportAt,
            Deadline           = r.Deadline
        };
    }
}
