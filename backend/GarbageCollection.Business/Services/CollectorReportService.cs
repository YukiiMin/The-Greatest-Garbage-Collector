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
            _staffRepository         = staffRepository;
            _teamRepository          = teamRepository;
            _reportRepository        = reportRepository;
            _citizenReportRepository = citizenReportRepository;
            _pointCategoryRepository = pointCategoryRepository;
            _uploadImageService      = uploadImageService;
            _sessionRepository       = sessionRepository;
        }

        public async Task<CollectorReportsResponseDto> GetTodayReportsAsync(Guid userId)
        {
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Staff record not found for this user.");

            if (staff.TeamId is null)
                throw new UnauthorizedAccessException("You are not assigned to any team.");

            var team = await _teamRepository.GetByIdAsync(staff.TeamId.Value)
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
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Staff record not found for this user.");

            if (staff.TeamId != teamId)
                throw new UnauthorizedAccessException("you do not belong to this team");

            var team = await _teamRepository.GetByIdAsync(teamId)
                ?? throw new KeyNotFoundException($"Team {teamId} not found.");

            // Check dispatch time

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

            var nowUtc      = DateTime.UtcNow;
            var dispatchUtc = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, dispatchHour, dispatchMinute, 0, DateTimeKind.Utc);
            if (nowUtc < dispatchUtc)
                throw new InvalidOperationException($"too early — shift starts at {team.DispatchTime ?? "20:00"}");

            var queued = await _reportRepository.CountAssignedTodayAsync(teamId, date);
            if (queued == 0)
                throw new KeyNotFoundException("no reports assigned for dispatch today");

            if (await _reportRepository.HasProcessingTodayAsync(teamId, date))
                throw new InvalidOperationException("shift already started today");

            var updated = await _reportRepository.StartShiftAsync(teamId, date);

            // Create TeamSession
            var session = new TeamSession
            {
                TeamId  = teamId,
                Date    = date,
                StartAt = nowUtc,
            };
            await _sessionRepository.CreateAsync(session);

            // Mark team as in-work
            team.InWork           = true;
            team.StartWorkingTime = nowUtc;
            await _teamRepository.UpdateAsync(team);

            return new StartShiftResponseDto
            {
                UpdatedCount = updated,
                AssignTime   = team.DispatchTime
            };
        }

        public async Task<CollectReportResponseDto> CollectReportAsync(Guid userId, Guid reportId, List<IFormFile> images)
        {
            var report = await _citizenReportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException($"report {reportId} not found");

            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new UnauthorizedAccessException("staff record not found");

            if (report.TeamId != staff.TeamId)
                throw new UnauthorizedAccessException("you are not in the team assigned to this report");

            if (report.Status != ReportStatus.Processing)
                throw new InvalidOperationException($"cannot collect report with status '{report.Status}'");

            var imageUrls = await _uploadImageService.UploadImagesAsync(images, "collector-reports");

            var points = 0;
            if (report.PointCategoryId.HasValue)
            {
                var category = await _pointCategoryRepository.GetByIdAsync(report.PointCategoryId.Value);
                if (category is not null)
                    points = CalculatePoints(report.Types, category.Mechanic);
            }

            var updated = await _reportRepository.CollectWithPointsAsync(report, imageUrls, points, null);

            return new CollectReportResponseDto
            {
                Id                 = updated.Id,
                Status             = updated.Status.ToString(),
                CollectorImageUrls = updated.CollectorImageUrls,
                CollectedAt        = updated.CollectedAt
            };
        }

        public async Task<CollectReportResponseDto> UpdateReportAsync(Guid userId, Guid reportId, UpdateReportRequest request)
        {
            var report = await _citizenReportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException($"report {reportId} not found");

            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new UnauthorizedAccessException("staff record not found");

            if (report.TeamId != staff.TeamId)
                throw new UnauthorizedAccessException("you are not in the team assigned to this report");

            if (report.Status != ReportStatus.Processing)
                throw new InvalidOperationException($"cannot update report with status '{report.Status}'");

            if (string.Equals(request.Status, "Collected", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Images is null || request.Images.Count == 0)
                    throw new ArgumentException("at least one image is required when marking as Collected");

                if (request.ActualCapacityKg is null or <= 0)
                    throw new ArgumentException("actual_capacity_kg must be greater than 0 when marking as Collected");

                var imageUrls = await _uploadImageService.UploadImagesAsync(request.Images, "collector-reports");

                var points = 0;
                if (report.PointCategoryId.HasValue)
                {
                    var category = await _pointCategoryRepository.GetByIdAsync(report.PointCategoryId.Value);
                    if (category is not null)
                        points = CalculatePoints(report.Types, category.Mechanic, request.ActualCapacityKg);
                }

                var updated = await _reportRepository.CollectWithPointsAsync(
                    report, imageUrls, points, request.ActualCapacityKg);

                return new CollectReportResponseDto
                {
                    Id                 = updated.Id,
                    Status             = updated.Status.ToString(),
                    CollectorImageUrls = updated.CollectorImageUrls,
                    CollectedAt        = updated.CollectedAt
                };
            }
            else if (string.Equals(request.Status, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                    throw new ArgumentException("reason is required when marking as Failed");

                await _reportRepository.MarkFailedAsync(report, request.Reason);

                return new CollectReportResponseDto
                {
                    Id                 = report.Id,
                    Status             = ReportStatus.Failed.ToString(),
                    CollectorImageUrls = report.CollectorImageUrls,
                    CollectedAt        = null
                };
            }
            else
            {
                throw new ArgumentException($"status must be 'Collected' or 'Failed', got '{request.Status}'");
            }
        }

        public async Task<EndShiftResponseDto> EndShiftAsync(Guid userId, Guid teamId, DateOnly date)
        {
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("staff record not found");

            if (staff.TeamId != teamId)
                throw new UnauthorizedAccessException("you do not belong to this team");

            var team = await _teamRepository.GetByIdAsync(teamId)
                ?? throw new KeyNotFoundException($"team {teamId} not found");

            if (!team.InWork)
                throw new InvalidOperationException("shift has not started yet");

            // Check that no reports are still Processing
            if (await _reportRepository.HasProcessingTodayAsync(teamId, date))
                throw new InvalidOperationException("cannot end shift — some reports are still being processed");

            var (totalReports, totalCapacity) = await _reportRepository.GetSessionSummaryAsync(teamId, date);

            var session = await _sessionRepository.GetByTeamAndDateAsync(teamId, date);
            var nowUtc  = DateTime.UtcNow;
            var durationMinutes = 0;

            if (session is not null)
            {
                durationMinutes         = (int)(nowUtc - session.StartAt).TotalMinutes;
                session.EndAt           = nowUtc;
                session.TotalReports    = totalReports;
                session.TotalCapacity   = totalCapacity;
                await _sessionRepository.UpdateAsync(session);
            }

            team.InWork        = false;
            team.LastFinishTime = nowUtc;
            await _teamRepository.UpdateAsync(team);

            return new EndShiftResponseDto
            {
                TotalReports           = totalReports,
                TotalCapacity          = totalCapacity,
                SessionDurationMinutes = durationMinutes
            };
        }

        public async Task<CollectorDashboardData> GetDashboardAsync(Guid userId)
        {
            var staff = await _staffRepository.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("staff record not found");

            if (staff.TeamId is null)
                throw new UnauthorizedAccessException("You are not assigned to any team.");

            var since    = staff.JoinTeamAt ?? DateTime.MinValue;
            var reports  = await _reportRepository.GetByTeamSinceAsync(staff.TeamId.Value, since);
            var sessions = await _sessionRepository.GetByTeamSinceAsync(staff.TeamId.Value, since);

            // ── Overview ──────────────────────────────────────────────────────
            var allTypes = Enum.GetValues<WasteType>();

            var overview = new OverviewDto
            {
                Total     = reports.Count,
                Collected = reports.Count(r => r.Status == ReportStatus.Collected),
                Failed    = reports.Count(r => r.Status == ReportStatus.Failed),
                ByType    = allTypes.Select(t => new TypeCountDto
                {
                    Type      = t.ToString(),
                    Collected = reports.Count(r => r.Status == ReportStatus.Collected && r.Types.Contains(t))
                }).ToList()
            };

            var capacityOverview = new CapacityOverviewDto
            {
                Total  = reports.Where(r => r.Status == ReportStatus.Collected).Sum(r => r.ActualCapacityKg ?? 0m),
                ByType = allTypes.Select(t => new TypeCapacityDto
                {
                    Type  = t.ToString(),
                    Total = reports
                        .Where(r => r.Status == ReportStatus.Collected && r.Types.Contains(t))
                        .Sum(r => r.ActualCapacityKg ?? 0m)
                }).ToList()
            };

            // ── Monthly Stats ─────────────────────────────────────────────────
            var monthlyStats = reports
                .GroupBy(r => r.ReportAt.ToString("yyyy-MM"))
                .Select(g => new MonthlyStatDto
                {
                    Month     = g.Key,
                    Total     = g.Count(),
                    Collected = g.Count(r => r.Status == ReportStatus.Collected),
                    Failed    = g.Count(r => r.Status == ReportStatus.Failed),
                    ByType    = allTypes.Select(t => new TypeCountDto
                    {
                        Type      = t.ToString(),
                        Collected = g.Count(r => r.Status == ReportStatus.Collected && r.Types.Contains(t))
                    }).ToList()
                })
                .OrderBy(x => x.Month)
                .ToList();

            var monthlyCapacity = reports
                .Where(r => r.Status == ReportStatus.Collected)
                .GroupBy(r => r.ReportAt.ToString("yyyy-MM"))
                .Select(g => new MonthlyCapacityDto
                {
                    Month  = g.Key,
                    Total  = g.Sum(r => r.ActualCapacityKg ?? 0m),
                    ByType = allTypes.Select(t => new TypeCapacityDto
                    {
                        Type  = t.ToString(),
                        Total = g.Where(r => r.Types.Contains(t)).Sum(r => r.ActualCapacityKg ?? 0m)
                    }).ToList()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // ── Daily Stats ───────────────────────────────────────────────────
            var dailyStats = reports
                .GroupBy(r => r.ReportAt.ToString("yyyy-MM-dd"))
                .Select(g => new DailyStatDto
                {
                    Date      = g.Key,
                    Total     = g.Count(),
                    Collected = g.Count(r => r.Status == ReportStatus.Collected),
                    Failed    = g.Count(r => r.Status == ReportStatus.Failed),
                    ByType    = allTypes.Select(t => new TypeCountDto
                    {
                        Type      = t.ToString(),
                        Collected = g.Count(r => r.Status == ReportStatus.Collected && r.Types.Contains(t))
                    }).ToList()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var dailyCapacity = reports
                .Where(r => r.Status == ReportStatus.Collected)
                .GroupBy(r => r.ReportAt.ToString("yyyy-MM-dd"))
                .Select(g => new DailyCapacityDto
                {
                    Date   = g.Key,
                    Total  = g.Sum(r => r.ActualCapacityKg ?? 0m),
                    ByType = allTypes.Select(t => new TypeCapacityDto
                    {
                        Type  = t.ToString(),
                        Total = g.Where(r => r.Types.Contains(t)).Sum(r => r.ActualCapacityKg ?? 0m)
                    }).ToList()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // ── Sessions ──────────────────────────────────────────────────────
            var sessionDays = sessions
                .GroupBy(s => s.Date.ToString("yyyy-MM-dd"))
                .Select(g => new SessionDayDto
                {
                    Date         = g.Key,
                    SessionCount = g.Count(),
                    Sessions     = g.Select(s => new SessionItemDto
                    {
                        StartAt       = s.StartAt,
                        EndAt         = s.EndAt,
                        TotalReports  = s.TotalReports,
                        TotalCapacity = s.TotalCapacity
                    }).ToList()
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return new CollectorDashboardData
            {
                Overview         = overview,
                CapacityOverview = capacityOverview,
                MonthlyStats     = monthlyStats,
                MonthlyCapacity  = monthlyCapacity,
                DailyStats       = dailyStats,
                DailyCapacity    = dailyCapacity,
                Sessions         = sessionDays
            };
        }

        /// <summary>
        /// Tính điểm dựa trên loại rác và mechanic.
        /// Nếu <paramref name="actualCapacityKg"/> được cung cấp, kiểm tra MinWeightGrams của từng loại:
        /// tổng trọng lượng (gram) phải đạt ngưỡng tối thiểu thì loại đó mới được điểm.
        /// </summary>
        private static int CalculatePoints(
            List<WasteType> types,
            PointMechanic mechanic,
            decimal? actualCapacityKg = null)
        {
            var actualGrams = actualCapacityKg.HasValue ? actualCapacityKg.Value * 1000 : (decimal?)null;

            var total = 0m;
            foreach (var type in types)
            {
                var criteria = type switch
                {
                    WasteType.Organic       => mechanic.Organic,
                    WasteType.Recyclable    => mechanic.Recyclable,
                    WasteType.NonRecyclable => mechanic.NonRecyclable,
                    _                       => null
                };

                if (criteria is null) continue;

                // Nếu có trọng lượng thực tế, kiểm tra ngưỡng tối thiểu
                if (actualGrams.HasValue && criteria.MinWeightGrams > 0 && actualGrams.Value < criteria.MinWeightGrams)
                    continue; // Chưa đủ kg → loại này không được điểm

                total += criteria.Points;
            }
            return (int)Math.Round(total);
        }

        private static CollectorReportItemDto MapToItem(CitizenReport r) => new()
        {
            Id              = r.Id,
            WasteCategories = r.Types.Select(t => t.ToString()).ToList(),
            WasteUnit       = r.Capacity,
            UserAddress     = r.User?.Address,
            Description     = r.Description,
            ImageUrls       = r.CitizenImageUrls,
            Status          = r.Status.ToString(),
            Deadline        = r.Deadline
        };
    }
}
