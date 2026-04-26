using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Enterprise;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using WasteType = GarbageCollection.Common.Enums.WasteType;

namespace GarbageCollection.Business.Services
{
    public sealed class EnterpriseService : IEnterpriseService
    {
        private readonly IEnterpriseRepository    _enterpriseRepository;
        private readonly IStaffRepository         _staffRepository;
        private readonly ICitizenReportRepository _reportRepository;
        private readonly ITeamRepository          _teamRepository;
        private readonly ICollectorRepository     _collectorRepository;
        private readonly IPointCategoryRepository _pointCategoryRepository;
        private readonly IWorkAreaRepository      _workAreaRepository;
        private readonly ITeamSessionRepository   _sessionRepository;
        private readonly ILogger<EnterpriseService> _logger;

        private static readonly IReadOnlySet<string> ValidStatuses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Pending", "Queue", "Assigned", "Processing",
                "Collected", "Completed", "Rejected", "Failed"
            };

        public EnterpriseService(
            IEnterpriseRepository    enterpriseRepository,
            IStaffRepository         staffRepository,
            ICitizenReportRepository reportRepository,
            ITeamRepository          teamRepository,
            ICollectorRepository     collectorRepository,
            IPointCategoryRepository pointCategoryRepository,
            IWorkAreaRepository      workAreaRepository,
            ITeamSessionRepository   sessionRepository,
            ILogger<EnterpriseService> logger)
        {
            _enterpriseRepository    = enterpriseRepository;
            _staffRepository         = staffRepository;
            _reportRepository        = reportRepository;
            _teamRepository          = teamRepository;
            _collectorRepository     = collectorRepository;
            _pointCategoryRepository = pointCategoryRepository;
            _workAreaRepository      = workAreaRepository;
            _sessionRepository       = sessionRepository;
            _logger                  = logger;
        }

        // ── Auth helper ───────────────────────────────────────────────────────

        private async Task<Enterprise?> GetEnterpriseAsync(string email)
            => await _enterpriseRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());

        private async Task<List<Guid>> GetTeamIdsAsync(Guid enterpriseId)
        {
            var staffs = await _staffRepository.GetByEnterpriseIdAsync(enterpriseId);
            return staffs
                .Where(s => s.TeamId.HasValue)
                .Select(s => s.TeamId!.Value)
                .Distinct()
                .ToList();
        }

        // ── GET /enterprise/dashboard ─────────────────────────────────────────

        public async Task<(int, ApiResponse<EnterpriseDashboardData>)> GetDashboardAsync(
            string email, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseDashboardData>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            // Load teams via collectors
            var collectors   = await _collectorRepository.GetByEnterpriseIdAsync(enterprise.Id);
            var collectorIds = collectors.Select(c => c.Id).ToList();
            var teams        = await _teamRepository.GetByCollectorIdsAsync(collectorIds);
            var teamIds      = teams.Select(t => t.Id).ToList();

            // Load reports then sessions sequentially (EF Core DbContext is not thread-safe)
            var reports  = await _reportRepository.GetAllForEnterpriseAsync(teamIds, ct);
            var sessions = await _sessionRepository.GetByTeamIdsAsync(teamIds, ct);

            var todayDate = DateTime.UtcNow.Date;

            // ── Today snapshot ────────────────────────────────────────────────
            var today = new EnterpriseTodayDto
            {
                Pending    = reports.Count(r => r.Status == ReportStatus.Pending),
                Queue      = reports.Count(r => r.Status == ReportStatus.Queue),
                Assigned   = reports.Count(r => r.Status == ReportStatus.Assigned),
                Processing = reports.Count(r => r.Status == ReportStatus.Processing),
                Collected  = reports.Count(r => r.Status == ReportStatus.Collected),
                Completed  = reports.Count(r => r.Status == ReportStatus.Completed
                                             && r.CompleteAt.HasValue
                                             && r.CompleteAt.Value.Date == todayDate),
                Failed     = reports.Count(r => r.Status == ReportStatus.Failed
                                             && r.UpdatedAt.HasValue
                                             && r.UpdatedAt.Value.Date == todayDate),
                Rejected   = reports.Count(r => r.Status == ReportStatus.Rejected
                                             && r.UpdatedAt.HasValue
                                             && r.UpdatedAt.Value.Date == todayDate),
                ActiveTeams = teams.Count(t => t.InWork)
            };

            // ── All-time summary ──────────────────────────────────────────────
            var completedReports = reports.Where(r => r.Status == ReportStatus.Collected
                                                   || r.Status == ReportStatus.Completed).ToList();
            var completedCount   = reports.Count(r => r.Status == ReportStatus.Completed);
            var failedCount      = reports.Count(r => r.Status == ReportStatus.Failed);
            var rejectedCount    = reports.Count(r => r.Status == ReportStatus.Rejected);
            var totalTerminated  = completedCount + failedCount + rejectedCount;
            var completionRate   = totalTerminated > 0
                ? Math.Round((decimal)completedCount / totalTerminated * 100, 1)
                : 0m;

            var avgHours = reports
                .Where(r => r.Status == ReportStatus.Completed && r.CompleteAt.HasValue)
                .Select(r => (r.CompleteAt!.Value - r.ReportAt).TotalHours)
                .DefaultIfEmpty(0)
                .Average();

            var totalKg = reports
                .Where(r => r.Status == ReportStatus.Completed)
                .Sum(r => r.ActualCapacityKg ?? 0m);

            var summary = new EnterpriseSummaryDto
            {
                Total              = reports.Count,
                Completed          = completedCount,
                Failed             = failedCount,
                Rejected           = rejectedCount,
                CompletionRate     = completionRate,
                AvgProcessingHours = Math.Round(avgHours, 1),
                TotalKg            = totalKg
            };

            // ── Capacity by waste type ────────────────────────────────────────
            var allTypes        = Enum.GetValues<WasteType>();
            var doneReports     = reports.Where(r => r.Status == ReportStatus.Completed).ToList();
            var capacity = new EnterpriseCapacityDto
            {
                TotalKg = totalKg,
                ByType  = allTypes.Select(t => new EnterpriseTypeCapacityDto
                {
                    Type    = t.ToString(),
                    TotalKg = doneReports
                        .Where(r => r.Types.Contains(t))
                        .Sum(r => r.ActualCapacityKg ?? 0m)
                }).ToList()
            };

            // ── Monthly breakdown ─────────────────────────────────────────────
            var monthly = reports
                .GroupBy(r => r.ReportAt.ToString("yyyy-MM"))
                .Select(g => new EnterpriseMonthlyDto
                {
                    Month     = g.Key,
                    Total     = g.Count(),
                    Completed = g.Count(r => r.Status == ReportStatus.Completed),
                    Failed    = g.Count(r => r.Status == ReportStatus.Failed),
                    Rejected  = g.Count(r => r.Status == ReportStatus.Rejected),
                    TotalKg   = g.Where(r => r.Status == ReportStatus.Completed)
                                 .Sum(r => r.ActualCapacityKg ?? 0m)
                })
                .OrderBy(x => x.Month)
                .ToList();

            // ── Team performance ──────────────────────────────────────────────
            var sessionsByTeam = sessions
                .GroupBy(s => s.TeamId)
                .ToDictionary(g => g.Key, g => g.Count());

            var teamStats = teams.Select(t =>
            {
                var teamReports = reports.Where(r => r.TeamId == t.Id).ToList();
                return new EnterpriseTeamPerformanceDto
                {
                    TeamId        = t.Id,
                    TeamName      = t.Name,
                    CollectorName = t.Collector?.Name ?? string.Empty,
                    Total         = teamReports.Count,
                    Completed     = teamReports.Count(r => r.Status == ReportStatus.Completed),
                    Failed        = teamReports.Count(r => r.Status == ReportStatus.Failed),
                    TotalKg       = teamReports
                        .Where(r => r.Status == ReportStatus.Completed)
                        .Sum(r => r.ActualCapacityKg ?? 0m),
                    SessionCount  = sessionsByTeam.GetValueOrDefault(t.Id, 0)
                };
            }).ToList();

            return (200, ApiResponse<EnterpriseDashboardData>.Success("success", new EnterpriseDashboardData
            {
                Today    = today,
                Summary  = summary,
                Capacity = capacity,
                Monthly  = monthly,
                Teams    = teamStats
            }));
        }

        // ── GET /enterprise/reports ───────────────────────────────────────────

        public async Task<(int, ApiResponse<EnterpriseReportListResponseDto>)> GetReportsAsync(
            string email, string? status, int page, int limit, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportListResponseDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            if (page < 1)
                return (422, ApiResponse<EnterpriseReportListResponseDto>.Fail(
                    "invalid page", "INVALID_QUERY", "page must be >= 1"));
            if (limit < 1 || limit > 100)
                return (422, ApiResponse<EnterpriseReportListResponseDto>.Fail(
                    "invalid limit", "INVALID_QUERY", "limit must be 1–100"));

            IEnumerable<ReportStatus>? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!ValidStatuses.Contains(status))
                    return (422, ApiResponse<EnterpriseReportListResponseDto>.Fail(
                        "invalid status", "INVALID_STATUS",
                        $"status must be one of: {string.Join(", ", ValidStatuses)}"));
                statusFilter = [Enum.Parse<ReportStatus>(status, ignoreCase: true)];
            }

            var teamIds = await GetTeamIdsAsync(enterprise.Id);

            var (items, total) = await _reportRepository.GetPagedForEnterpriseAsync(
                teamIds, statusFilter, page, limit, ct);

            var dtos = items.Select(MapToDto).ToList();

            return (200, ApiResponse<EnterpriseReportListResponseDto>.Success("success",
                new EnterpriseReportListResponseDto
                {
                    Reports    = dtos,
                    Pagination = new PaginationMeta { Page = page, Limit = limit, Total = total }
                }));
        }

        // ── GET /enterprise/reports/{id} ──────────────────────────────────────

        public async Task<(int, ApiResponse<EnterpriseReportDto>)> GetReportDetailAsync(
            string email, Guid reportId, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            // Verify visibility: no team OR team belongs to this enterprise
            if (report.TeamId.HasValue)
            {
                var teamIds = await GetTeamIdsAsync(enterprise.Id);
                if (!teamIds.Contains(report.TeamId.Value))
                    return (403, ApiResponse<EnterpriseReportDto>.Fail(
                        "forbidden", "FORBIDDEN", "Report does not belong to your enterprise"));
            }

            return (200, ApiResponse<EnterpriseReportDto>.Success("success", MapToDto(report)));
        }

        // ── PATCH /enterprise/reports/{id}/queue ──────────────────────────────

        public async Task<(int, ApiResponse<EnterpriseReportDto>)> QueueReportAsync(
            string email, Guid reportId, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdTrackedAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.Status != ReportStatus.Pending)
                return (409, ApiResponse<EnterpriseReportDto>.Fail(
                    "invalid status transition", "INVALID_STATUS",
                    $"Report must be Pending to queue, current status: {report.Status}"));

            report.Status    = ReportStatus.Queue;
            report.UpdatedAt = DateTime.UtcNow;
            await _reportRepository.UpdateAsync(report);

            return (200, ApiResponse<EnterpriseReportDto>.Success("report queued", MapToDto(report)));
        }

        // ── PATCH /enterprise/reports/{id}/assign ─────────────────────────────

        public async Task<(int, ApiResponse<EnterpriseReportDto>)> AssignReportAsync(
            string email, Guid reportId, AssignReportRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdTrackedAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.Status != ReportStatus.Queue)
                return (409, ApiResponse<EnterpriseReportDto>.Fail(
                    "invalid status transition", "INVALID_STATUS",
                    $"Report must be Queue to assign, current status: {report.Status}"));

            // Validate team belongs to this enterprise
            var teamIds = await GetTeamIdsAsync(enterprise.Id);
            if (!teamIds.Contains(request.Data.TeamId))
                return (422, ApiResponse<EnterpriseReportDto>.Fail(
                    "invalid team", "INVALID_TEAM",
                    "Team does not belong to your enterprise"));

            if (request.Data.Deadline <= DateTime.UtcNow)
                return (422, ApiResponse<EnterpriseReportDto>.Fail(
                    "invalid deadline", "INVALID_DEADLINE",
                    "Deadline must be in the future"));

            report.Status    = ReportStatus.Assigned;
            report.TeamId    = request.Data.TeamId;
            report.Deadline  = request.Data.Deadline;
            report.AssignAt  = DateTime.UtcNow;
            report.AssignBy  = enterprise.Id;
            report.UpdatedAt = DateTime.UtcNow;
            await _reportRepository.UpdateAsync(report);

            return (200, ApiResponse<EnterpriseReportDto>.Success("report assigned", MapToDto(report)));
        }

        // ── PATCH /enterprise/reports/{id}/reject ─────────────────────────────

        public async Task<(int, ApiResponse<EnterpriseReportDto>)> RejectReportAsync(
            string email, Guid reportId, RejectReportRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdTrackedAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.Status != ReportStatus.Pending && report.Status != ReportStatus.Queue)
                return (409, ApiResponse<EnterpriseReportDto>.Fail(
                    "invalid status transition", "INVALID_STATUS",
                    $"Report must be Pending or Queue to reject, current status: {report.Status}"));

            report.Status     = ReportStatus.Rejected;
            report.ReportNote = request.Data.Reason;
            report.UpdatedAt  = DateTime.UtcNow;
            await _reportRepository.UpdateAsync(report);

            return (200, ApiResponse<EnterpriseReportDto>.Success("report rejected", MapToDto(report)));
        }

        // ── PATCH /enterprise/reports/{id}/complete ───────────────────────────

        public async Task<(int, ApiResponse<EnterpriseReportDto>)> CompleteReportAsync(
            string email, Guid reportId, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdTrackedAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.Status != ReportStatus.Collected)
                return (409, ApiResponse<EnterpriseReportDto>.Fail(
                    "invalid status transition", "INVALID_STATUS",
                    $"Report must be Collected to complete, current status: {report.Status}"));

            // Verify the report's team belongs to this enterprise
            if (report.TeamId.HasValue)
            {
                var teamIds = await GetTeamIdsAsync(enterprise.Id);
                if (!teamIds.Contains(report.TeamId.Value))
                    return (403, ApiResponse<EnterpriseReportDto>.Fail(
                        "forbidden", "FORBIDDEN", "Report does not belong to your enterprise"));
            }

            report.Status     = ReportStatus.Completed;
            report.CompleteAt = DateTime.UtcNow;
            report.UpdatedAt  = DateTime.UtcNow;
            await _reportRepository.UpdateAsync(report);

            return (200, ApiResponse<EnterpriseReportDto>.Success("report completed", MapToDto(report)));
        }

        // ── GET /enterprise/collectors ────────────────────────────────────────

        public async Task<(int, ApiResponse<List<CollectorDto>>)> GetCollectorsAsync(
            string email, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<List<CollectorDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collectors = await _collectorRepository.GetByEnterpriseIdAsync(enterprise.Id);
            return (200, ApiResponse<List<CollectorDto>>.Success("success",
                collectors.Select(MapToCollectorDto).ToList()));
        }

        // ── GET /enterprise/collectors/{id} ──────────────────────────────────

        public async Task<(int, ApiResponse<CollectorDto>)> GetCollectorDetailAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collector = await _collectorRepository.GetByIdAsync(id);
            if (collector is null || collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<CollectorDto>.Fail(
                    "not found", "NOT_FOUND", "Collector does not exist"));

            return (200, ApiResponse<CollectorDto>.Success("success", MapToCollectorDto(collector)));
        }

        // ── POST /enterprise/collectors ───────────────────────────────────────

        public async Task<(int, ApiResponse<CollectorDto>)> CreateCollectorAsync(
            string email, SaveCollectorRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            // Validate work area: Ward must belong to enterprise's District
            if (request.Data.WorkAreaId.HasValue)
            {
                var workArea = await _workAreaRepository.GetByIdAsync(request.Data.WorkAreaId.Value);
                if (workArea is null)
                    return (422, ApiResponse<CollectorDto>.Fail(
                        "invalid work area", "INVALID_WORK_AREA", "Work area not found"));
                if (workArea.Type != "Ward")
                    return (422, ApiResponse<CollectorDto>.Fail(
                        "invalid work area", "INVALID_WORK_AREA", "Collector work area must be a Ward"));
                if (enterprise.WorkAreaId.HasValue && workArea.ParentId != enterprise.WorkAreaId)
                    return (409, ApiResponse<CollectorDto>.Fail(
                        "work area mismatch", "WORK_AREA_MISMATCH",
                        "This ward does not belong to the enterprise's district"));
            }

            var collector = new Collector
            {
                Name             = request.Data.Name.Trim(),
                PhoneNumber      = request.Data.PhoneNumber.Trim(),
                Email            = request.Data.Email.Trim().ToLowerInvariant(),
                Address          = request.Data.Address.Trim(),
                Latitude         = request.Data.Latitude,
                Longitude        = request.Data.Longitude,
                WorkAreaId       = request.Data.WorkAreaId,
                AssignedCapacity = request.Data.AssignedCapacity,
                EnterpriseId     = enterprise.Id
            };

            var created = await _collectorRepository.CreateAsync(collector);
            return (201, ApiResponse<CollectorDto>.Success("collector created", MapToCollectorDto(created)));
        }

        // ── PATCH /enterprise/collectors/{id} ────────────────────────────────

        public async Task<(int, ApiResponse<CollectorDto>)> UpdateCollectorAsync(
            string email, Guid id, SaveCollectorRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collector = await _collectorRepository.GetByIdAsync(id);
            if (collector is null || collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<CollectorDto>.Fail(
                    "not found", "NOT_FOUND", "Collector does not exist"));

            // Validate work area: Ward must belong to enterprise's District
            if (request.Data.WorkAreaId.HasValue)
            {
                var workArea = await _workAreaRepository.GetByIdAsync(request.Data.WorkAreaId.Value);
                if (workArea is null)
                    return (422, ApiResponse<CollectorDto>.Fail(
                        "invalid work area", "INVALID_WORK_AREA", "Work area not found"));
                if (workArea.Type != "Ward")
                    return (422, ApiResponse<CollectorDto>.Fail(
                        "invalid work area", "INVALID_WORK_AREA", "Collector work area must be a Ward"));
                if (enterprise.WorkAreaId.HasValue && workArea.ParentId != enterprise.WorkAreaId)
                    return (409, ApiResponse<CollectorDto>.Fail(
                        "work area mismatch", "WORK_AREA_MISMATCH",
                        "This ward does not belong to the enterprise's district"));
            }

            collector.Name        = request.Data.Name.Trim();
            collector.PhoneNumber = request.Data.PhoneNumber.Trim();
            collector.Email       = request.Data.Email.Trim().ToLowerInvariant();
            collector.Address     = request.Data.Address.Trim();
            if (request.Data.Latitude.HasValue)         collector.Latitude         = request.Data.Latitude;
            if (request.Data.Longitude.HasValue)        collector.Longitude        = request.Data.Longitude;
            if (request.Data.WorkAreaId.HasValue)       collector.WorkAreaId       = request.Data.WorkAreaId;
            if (request.Data.AssignedCapacity.HasValue) collector.AssignedCapacity = request.Data.AssignedCapacity;

            var updated = await _collectorRepository.UpdateAsync(collector);
            return (200, ApiResponse<CollectorDto>.Success("collector updated", MapToCollectorDto(updated)));
        }

        // ── DELETE /enterprise/collectors/{id} ───────────────────────────────

        public async Task<(int, ApiResponse<object>)> DeleteCollectorAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collector = await _collectorRepository.GetByIdAsync(id);
            if (collector is null || collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Collector does not exist"));

            var teams = await _teamRepository.GetByCollectorIdAsync(id);
            if (teams.Any())
                return (409, ApiResponse<object>.Fail(
                    "collector has teams", "COLLECTOR_HAS_TEAMS",
                    "Remove all teams from this collector before deleting"));

            await _collectorRepository.DeleteAsync(collector);
            return (200, ApiResponse<object>.Success("collector deleted", null!));
        }

        // ── GET /enterprise/teams ─────────────────────────────────────────────

        public async Task<(int, ApiResponse<List<TeamDetailDto>>)> GetTeamsAsync(
            string email, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<List<TeamDetailDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collectors = await _collectorRepository.GetByEnterpriseIdAsync(enterprise.Id);
            var collectorIds = collectors.Select(c => c.Id).ToList();

            var teams = await _teamRepository.GetByCollectorIdsAsync(collectorIds);

            var staffs = await _staffRepository.GetByEnterpriseIdAsync(enterprise.Id);
            var memberCounts = staffs
                .Where(s => s.TeamId.HasValue)
                .GroupBy(s => s.TeamId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var dtos = teams.Select(t => MapToTeamDetailDto(t, memberCounts.GetValueOrDefault(t.Id, 0))).ToList();
            return (200, ApiResponse<List<TeamDetailDto>>.Success("success", dtos));
        }

        // ── GET /enterprise/teams/{id} ────────────────────────────────────────

        public async Task<(int, ApiResponse<TeamDetailDto>)> GetTeamDetailAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<TeamDetailDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team = await _teamRepository.GetByIdAsync(id);
            if (team is null || team.Collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<TeamDetailDto>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            var staffs = await _staffRepository.GetByTeamIdAsync(id);
            return (200, ApiResponse<TeamDetailDto>.Success("success",
                MapToTeamDetailDto(team, staffs.Count())));
        }

        // ── POST /enterprise/teams ────────────────────────────────────────────

        public async Task<(int, ApiResponse<TeamDetailDto>)> CreateTeamAsync(
            string email, SaveTeamRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<TeamDetailDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collector = await _collectorRepository.GetByIdAsync(request.Data.CollectorId);
            if (collector is null || collector.EnterpriseId != enterprise.Id)
                return (422, ApiResponse<TeamDetailDto>.Fail(
                    "invalid collector", "INVALID_COLLECTOR",
                    "Collector does not exist or does not belong to your enterprise"));

            var team = new Team
            {
                Name          = request.Data.Name.Trim(),
                CollectorId   = request.Data.CollectorId,
                TotalCapacity = request.Data.TotalCapacity,
                IsActive      = request.Data.IsActive,
                DispatchTime  = request.Data.DispatchTime?.Trim()
            };

            var created = await _teamRepository.CreateAsync(team);
            // Reload to include Collector navigation
            var reloaded = await _teamRepository.GetByIdAsync(created.Id);
            return (201, ApiResponse<TeamDetailDto>.Success("team created",
                MapToTeamDetailDto(reloaded!, 0)));
        }

        // ── PATCH /enterprise/teams/{id} ──────────────────────────────────────

        public async Task<(int, ApiResponse<TeamDetailDto>)> UpdateTeamAsync(
            string email, Guid id, SaveTeamRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<TeamDetailDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team = await _teamRepository.GetByIdAsync(id);
            if (team is null || team.Collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<TeamDetailDto>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            // If changing collector, verify new collector also belongs to enterprise
            if (request.Data.CollectorId != team.CollectorId)
            {
                var newCollector = await _collectorRepository.GetByIdAsync(request.Data.CollectorId);
                if (newCollector is null || newCollector.EnterpriseId != enterprise.Id)
                    return (422, ApiResponse<TeamDetailDto>.Fail(
                        "invalid collector", "INVALID_COLLECTOR",
                        "Collector does not exist or does not belong to your enterprise"));
            }

            team.Name          = request.Data.Name.Trim();
            team.CollectorId   = request.Data.CollectorId;
            team.TotalCapacity = request.Data.TotalCapacity;
            team.IsActive      = request.Data.IsActive;
            team.DispatchTime  = request.Data.DispatchTime?.Trim();

            var updated = await _teamRepository.UpdateAsync(team);
            var staffs = await _staffRepository.GetByTeamIdAsync(id);
            return (200, ApiResponse<TeamDetailDto>.Success("team updated",
                MapToTeamDetailDto(updated, staffs.Count())));
        }

        // ── DELETE /enterprise/teams/{id} ─────────────────────────────────────

        public async Task<(int, ApiResponse<object>)> DeleteTeamAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team = await _teamRepository.GetByIdAsync(id);
            if (team is null || team.Collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            var staffs = await _staffRepository.GetByTeamIdAsync(id);
            if (staffs.Any())
                return (409, ApiResponse<object>.Fail(
                    "team has staff", "TEAM_HAS_STAFF",
                    "Remove all staff members from this team before deleting"));

            await _teamRepository.DeleteAsync(team);
            return (200, ApiResponse<object>.Success("team deleted", null!));
        }

        // ── GET /enterprise/point-categories ─────────────────────────────────

        public async Task<(int, ApiResponse<List<PointCategoryDto>>)> GetPointCategoriesAsync(
            string email, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<List<PointCategoryDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var categories = await _pointCategoryRepository.GetByEnterpriseIdAsync(enterprise.Id);
            var dtos = categories.Select(MapToCategoryDto).ToList();

            return (200, ApiResponse<List<PointCategoryDto>>.Success("success", dtos));
        }

        // ── POST /enterprise/point-categories ────────────────────────────────

        public async Task<(int, ApiResponse<PointCategoryDto>)> CreatePointCategoryAsync(
            string email, SavePointCategoryRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<PointCategoryDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var category = new PointCategory
            {
                Name         = request.Data.Name.Trim(),
                Mechanic     = request.Data.Mechanic,
                IsActive     = request.Data.IsActive,
                EnterpriseId = enterprise.Id
            };

            var created = await _pointCategoryRepository.CreateAsync(category);
            return (201, ApiResponse<PointCategoryDto>.Success("point category created", MapToCategoryDto(created)));
        }

        // ── PATCH /enterprise/point-categories/{id} ───────────────────────────

        public async Task<(int, ApiResponse<PointCategoryDto>)> UpdatePointCategoryAsync(
            string email, Guid id, SavePointCategoryRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<PointCategoryDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var category = await _pointCategoryRepository.GetByIdAsync(id);
            if (category is null)
                return (404, ApiResponse<PointCategoryDto>.Fail(
                    "not found", "NOT_FOUND", "Point category does not exist"));

            if (category.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<PointCategoryDto>.Fail(
                    "forbidden", "FORBIDDEN", "Point category does not belong to your enterprise"));

            category.Name     = request.Data.Name.Trim();
            category.Mechanic = request.Data.Mechanic;
            category.IsActive = request.Data.IsActive;

            var updated = await _pointCategoryRepository.UpdateAsync(category);
            return (200, ApiResponse<PointCategoryDto>.Success("point category updated", MapToCategoryDto(updated)));
        }

        // ── DELETE /enterprise/point-categories/{id} ──────────────────────────

        public async Task<(int, ApiResponse<object>)> DeletePointCategoryAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var category = await _pointCategoryRepository.GetByIdAsync(id);
            if (category is null)
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Point category does not exist"));

            if (category.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<object>.Fail(
                    "forbidden", "FORBIDDEN", "Point category does not belong to your enterprise"));

            await _pointCategoryRepository.DeleteAsync(category);
            return (200, ApiResponse<object>.Success("point category deleted", null!));
        }

        // ── Mappers ───────────────────────────────────────────────────────────

        private static EnterpriseReportDto MapToDto(CitizenReport r) => new()
        {
            Id                = r.Id,
            TeamId            = r.TeamId,
            Types             = r.Types.Select(t => t.ToString()).ToList(),
            Capacity          = r.Capacity,
            ActualCapacityKg  = r.ActualCapacityKg,
            Status            = r.Status.ToString(),
            CitizenEmail      = r.User?.Email ?? string.Empty,
            Description       = r.Description,
            ReportNote        = r.ReportNote,
            CitizenImageUrls  = r.CitizenImageUrls,
            CollectorImageUrls = r.CollectorImageUrls,
            ReportAt          = r.ReportAt,
            AssignAt          = r.AssignAt,
            Deadline          = r.Deadline,
            CollectedAt       = r.CollectedAt,
            CompleteAt        = r.CompleteAt
        };

        private static PointCategoryDto MapToCategoryDto(PointCategory c) => new()
        {
            Id        = c.Id,
            Name      = c.Name,
            Mechanic  = c.Mechanic,
            IsActive  = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };

        private static CollectorDto MapToCollectorDto(Collector c) => new()
        {
            Id               = c.Id,
            Name             = c.Name,
            PhoneNumber      = c.PhoneNumber,
            Email            = c.Email,
            Address          = c.Address,
            Latitude         = c.Latitude,
            Longitude        = c.Longitude,
            WorkAreaId       = c.WorkAreaId,
            WorkAreaName     = c.WorkArea?.Name,
            AssignedCapacity = c.AssignedCapacity,
            CreatedAt        = c.CreatedAt,
            UpdatedAt        = c.UpdatedAt
        };

        private static TeamDetailDto MapToTeamDetailDto(Team t, int memberCount) => new()
        {
            Id            = t.Id,
            Name          = t.Name,
            InWork        = t.InWork,
            IsActive      = t.IsActive,
            TotalCapacity = t.TotalCapacity,
            CollectorId   = t.CollectorId,
            CollectorName = t.Collector?.Name ?? string.Empty,
            DispatchTime  = t.DispatchTime,
            MemberCount   = memberCount,
            CreatedAt     = t.CreatedAt,
            UpdatedAt     = t.UpdatedAt
        };

        // ── GET /enterprise/teams/{teamId}/staff ──────────────────────────────

        public async Task<(int, ApiResponse<List<StaffDto>>)> GetTeamStaffAsync(
            string email, Guid teamId, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<List<StaffDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team is null || team.Collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<List<StaffDto>>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            var staffs = await _staffRepository.GetByTeamIdAsync(teamId);
            return (200, ApiResponse<List<StaffDto>>.Success("success",
                staffs.Select(MapToStaffDto).ToList()));
        }

        // ── POST /enterprise/teams/{teamId}/staff ─────────────────────────────

        public async Task<(int, ApiResponse<StaffDto>)> AddTeamStaffAsync(
            string email, Guid teamId, AddStaffRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<StaffDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team is null || team.Collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<StaffDto>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            var staff = await _staffRepository.GetByUserIdAsync(request.UserId);
            if (staff is null)
                return (404, ApiResponse<StaffDto>.Fail(
                    "staff not found", "NOT_FOUND",
                    "User must be set up as a collector by admin first"));

            if (staff.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<StaffDto>.Fail(
                    "forbidden", "FORBIDDEN",
                    "This staff member belongs to a different enterprise"));

            if (staff.TeamId.HasValue)
                return (409, ApiResponse<StaffDto>.Fail(
                    "already assigned to a team", "ALREADY_IN_TEAM",
                    "Remove staff from their current team first"));

            staff.CollectorId = team.CollectorId;
            staff.TeamId      = teamId;
            staff.JoinTeamAt  = DateTime.UtcNow;

            var updated = await _staffRepository.UpdateAsync(staff);
            // Reload with User navigation
            var reloaded = await _staffRepository.GetByUserIdAsync(updated.UserId);
            return (200, ApiResponse<StaffDto>.Success("staff assigned to team", MapToStaffDto(reloaded!)));
        }

        // ── DELETE /enterprise/teams/{teamId}/staff/{userId} ──────────────────

        public async Task<(int, ApiResponse<object>)> RemoveTeamStaffAsync(
            string email, Guid teamId, Guid userId, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email);
            if (enterprise is null)
                return (401, ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var staff = await _staffRepository.GetByUserIdAsync(userId);
            if (staff is null || staff.TeamId != teamId)
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Staff member not found in this team"));

            if (staff.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<object>.Fail(
                    "forbidden", "FORBIDDEN", "Staff member does not belong to your enterprise"));

            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team is null || team.Collector.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<object>.Fail(
                    "forbidden", "FORBIDDEN", "Team does not belong to your enterprise"));

            // Clear team assignment — staff remains in enterprise as unassigned collector
            staff.CollectorId = null;
            staff.TeamId      = null;
            staff.JoinTeamAt  = null;
            await _staffRepository.UpdateAsync(staff);

            return (200, ApiResponse<object>.Success("staff removed from team", null!));
        }

        private static StaffDto MapToStaffDto(Staff s) => new()
        {
            UserId       = s.UserId,
            UserEmail    = s.User?.Email ?? string.Empty,
            UserFullName = s.User?.FullName ?? string.Empty,
            CollectorId  = s.CollectorId,
            TeamId       = s.TeamId,
            JoinTeamAt   = s.JoinTeamAt
        };
    }
}
