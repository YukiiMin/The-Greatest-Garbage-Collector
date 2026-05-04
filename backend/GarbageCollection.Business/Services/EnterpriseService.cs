    using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Enterprise;
using GarbageCollection.Common.DTOs.Staff;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using WasteType = GarbageCollection.Common.Enums.WasteType;
using CollectorDtoNs = GarbageCollection.Common.DTOs.Collector;

namespace GarbageCollection.Business.Services
{
    public sealed class EnterpriseService : IEnterpriseService
    {
        private readonly IEnterpriseRepository      _enterpriseRepository;
        private readonly IEnterpriseStaffRepository  _enterpriseStaffRepository;
        private readonly ICitizenReportRepository    _reportRepository;
        private readonly ITeamRepository             _teamRepository;
        private readonly ICollectorRepository        _collectorRepository;
        private readonly ICollectorHubRepository     _collectorHubRepository;
        private readonly ICollectorStaffRepository   _collectorStaffRepository;
        private readonly IPointCategoryRepository    _pointCategoryRepository;
        private readonly IWorkAreaRepository         _workAreaRepository;
        private readonly ITeamSessionRepository      _sessionRepository;
        private readonly IUserRepository             _userRepository;
        private readonly ILogger<EnterpriseService>  _logger;

        private static readonly IReadOnlySet<string> ValidStatuses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Pending", "Queue", "Assigned", "Processing",
                "Collected", "Completed", "Rejected", "Failed"
            };

        public EnterpriseService(
            IEnterpriseRepository      enterpriseRepository,
            IEnterpriseStaffRepository  enterpriseStaffRepository,
            ICitizenReportRepository    reportRepository,
            ITeamRepository             teamRepository,
            ICollectorRepository        collectorRepository,
            ICollectorHubRepository     collectorHubRepository,
            ICollectorStaffRepository   collectorStaffRepository,
            IPointCategoryRepository    pointCategoryRepository,
            IWorkAreaRepository         workAreaRepository,
            ITeamSessionRepository      sessionRepository,
            IUserRepository             userRepository,
            ILogger<EnterpriseService>  logger)
        {
            _enterpriseRepository      = enterpriseRepository;
            _enterpriseStaffRepository  = enterpriseStaffRepository;
            _reportRepository           = reportRepository;
            _teamRepository             = teamRepository;
            _collectorRepository        = collectorRepository;
            _collectorHubRepository     = collectorHubRepository;
            _collectorStaffRepository   = collectorStaffRepository;
            _pointCategoryRepository    = pointCategoryRepository;
            _workAreaRepository         = workAreaRepository;
            _sessionRepository          = sessionRepository;
            _userRepository             = userRepository;
            _logger                     = logger;
        }

        // ── Auth helpers ──────────────────────────────────────────────────────

        private async Task<Enterprise?> GetEnterpriseAsync(string email, CancellationToken ct = default)
        {
            var (enterprise, _) = await GetEnterpriseAndStaffAsync(email, ct);
            return enterprise;
        }


        /// <summary>Resolve enterprise for any Enterprise-role user (main user or staff).</summary>
        private async Task<(Enterprise? enterprise, EnterpriseStaff? staff)> GetEnterpriseAndStaffAsync(
            string email, CancellationToken ct)
        {
            var normalised = email.Trim().ToLowerInvariant();

            // Try direct enterprise email first (main enterprise user)
            var enterprise = await _enterpriseRepository.GetByEmailAsync(normalised);
            if (enterprise is not null)
                return (enterprise, null);

            // Fallback: look up via EnterpriseStaff record
            var user = await _userRepository.GetByEmailAsync(normalised, ct);
            if (user is null) return (null, null);

            var staff = await _enterpriseStaffRepository.GetByUserIdAsync(user.Id);
            if (staff is null) return (null, null);

            enterprise = await _enterpriseRepository.GetByIdAsync(staff.EnterpriseId);
            return (enterprise, staff);
        }

        private async Task<List<Guid>> GetTeamIdsAsync(Guid enterpriseId)
        {
            // Enterprise → Collectors → CollectorStaff → Teams (via TeamId and CollectorHubId)
            var collectors = await _collectorRepository.GetByEnterpriseIdAsync(enterpriseId);
            var teamIds = new HashSet<Guid>();
            var hubIds  = new HashSet<Guid>();

            foreach (var c in collectors)
            {
                var staffList = await _collectorStaffRepository.GetByCollectorIdAsync(c.Id);
                foreach (var s in staffList)
                {
                    if (s.TeamId.HasValue)         teamIds.Add(s.TeamId.Value);
                    if (s.CollectorHubId.HasValue) hubIds.Add(s.CollectorHubId.Value);
                }
            }

            // Also include teams under hubs where enterprise staff work (covers newly-created empty teams)
            if (hubIds.Count > 0)
            {
                var hubTeams = await _teamRepository.GetByCollectorHubIdsAsync(hubIds);
                foreach (var t in hubTeams) teamIds.Add(t.Id);
            }

            return teamIds.ToList();
        }

        // ── GET /enterprise/dashboard ─────────────────────────────────────────

        public async Task<(int, ApiResponse<EnterpriseDashboardData>)> GetDashboardAsync(
            string email, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseDashboardData>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            // Load teams via Collectors → CollectorStaff → Teams
            var teamIds = await GetTeamIdsAsync(enterprise.Id);
            var teams   = await _teamRepository.GetByIdsAsync(teamIds);

            // Load reports then sessions sequentially (EF Core DbContext is not thread-safe)
            var reports  = await _reportRepository.GetAllForEnterpriseAsync(enterprise.Id, teamIds, ct);
            var sessions = await _sessionRepository.GetByTeamIdsAsync(teamIds, ct);

            var todayDate = DateTime.UtcNow.Date;

            // ── Today snapshot (chỉ reports được tạo hôm nay) ────────────────
            var todayReports = reports.Where(r => r.ReportAt.Date == todayDate).ToList();
            var today = new EnterpriseTodayDto
            {
                // Pipeline hiện tại của reports tạo hôm nay
                Pending    = todayReports.Count(r => r.Status == ReportStatus.Pending),
                Queue      = todayReports.Count(r => r.Status == ReportStatus.Queue),
                Assigned   = todayReports.Count(r => r.Status == ReportStatus.Assigned),
                Processing = todayReports.Count(r => r.Status == ReportStatus.Processing),
                Collected  = todayReports.Count(r => r.Status == ReportStatus.Collected),
                // Công việc thực tế hoàn thành / thất bại hôm nay (toàn bộ lịch sử, không chỉ hôm nay)
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
            var completedCount  = reports.Count(r => r.Status == ReportStatus.Completed);
            var failedCount     = reports.Count(r => r.Status == ReportStatus.Failed);
            var rejectedCount   = reports.Count(r => r.Status == ReportStatus.Rejected);
            var totalTerminated = completedCount + failedCount + rejectedCount;
            var completionRate  = totalTerminated > 0
                ? Math.Round((decimal)completedCount / totalTerminated * 100, 1)
                : 0m;

            // Thời gian xử lý thực tế: từ lúc assign cho team đến lúc collector thu gom xong
            var avgHours = reports
                .Where(r => r.Status == ReportStatus.Completed
                         && r.CollectedAt.HasValue
                         && r.AssignAt.HasValue)
                .Select(r => (r.CollectedAt!.Value - r.AssignAt!.Value).TotalHours)
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
            var allTypes    = Enum.GetValues<WasteType>();
            var doneReports = reports.Where(r => r.Status == ReportStatus.Completed).ToList();
            var capacity    = new EnterpriseCapacityDto
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
            // Total: theo tháng report được tạo (intake)
            // Completed/Failed/Rejected/TotalKg: theo tháng công việc thực sự hoàn thành
            var intakeMonths = reports
                .Select(r => r.ReportAt.ToString("yyyy-MM"));
            var outputMonths = reports
                .Where(r => r.CompleteAt.HasValue || (r.UpdatedAt.HasValue &&
                            (r.Status == ReportStatus.Failed || r.Status == ReportStatus.Rejected)))
                .Select(r => r.CompleteAt.HasValue
                    ? r.CompleteAt!.Value.ToString("yyyy-MM")
                    : r.UpdatedAt!.Value.ToString("yyyy-MM"));
            var allMonths = intakeMonths.Concat(outputMonths).Distinct().OrderBy(x => x).ToList();

            var monthly = allMonths.Select(month => new EnterpriseMonthlyDto
            {
                Month     = month,
                Total     = reports.Count(r => r.ReportAt.ToString("yyyy-MM") == month),
                Completed = reports.Count(r => r.Status == ReportStatus.Completed
                                            && r.CompleteAt.HasValue
                                            && r.CompleteAt.Value.ToString("yyyy-MM") == month),
                Failed    = reports.Count(r => r.Status == ReportStatus.Failed
                                            && r.UpdatedAt.HasValue
                                            && r.UpdatedAt.Value.ToString("yyyy-MM") == month),
                Rejected  = reports.Count(r => r.Status == ReportStatus.Rejected
                                            && r.UpdatedAt.HasValue
                                            && r.UpdatedAt.Value.ToString("yyyy-MM") == month),
                TotalKg   = reports.Where(r => r.Status == ReportStatus.Completed
                                            && r.CompleteAt.HasValue
                                            && r.CompleteAt.Value.ToString("yyyy-MM") == month)
                                   .Sum(r => r.ActualCapacityKg ?? 0m)
            }).ToList();

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
                    CollectorName = string.Empty,
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
            var enterprise = await GetEnterpriseAsync(email, ct);
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
                enterprise.Id, teamIds, statusFilter, page, limit, ct);

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
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<EnterpriseReportDto>.Fail(
                    "forbidden", "FORBIDDEN", "Report does not belong to your enterprise"));

            return (200, ApiResponse<EnterpriseReportDto>.Success("success", MapToDto(report)));
        }

        // ── PATCH /enterprise/reports/{id}/queue ──────────────────────────────

        public async Task<(int, ApiResponse<EnterpriseReportDto>)> QueueReportAsync(
            string email, Guid reportId, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdTrackedAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<EnterpriseReportDto>.Fail(
                    "forbidden", "FORBIDDEN", "Report does not belong to your enterprise"));

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
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdTrackedAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<EnterpriseReportDto>.Fail(
                    "forbidden", "FORBIDDEN", "Report does not belong to your enterprise"));

            if (report.Status != ReportStatus.Queue)
                return (409, ApiResponse<EnterpriseReportDto>.Fail(
                    "invalid status transition", "INVALID_STATUS",
                    $"Report must be Queue to assign, current status: {report.Status}"));

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
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdTrackedAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<EnterpriseReportDto>.Fail(
                    "forbidden", "FORBIDDEN", "Report does not belong to your enterprise"));

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
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<EnterpriseReportDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var report = await _reportRepository.GetByIdTrackedAsync(reportId);
            if (report is null)
                return (404, ApiResponse<EnterpriseReportDto>.Fail(
                    "report not found", "NOT_FOUND", "Report does not exist"));

            if (report.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<EnterpriseReportDto>.Fail(
                    "forbidden", "FORBIDDEN", "Report does not belong to your enterprise"));

            if (report.Status != ReportStatus.Collected)
                return (409, ApiResponse<EnterpriseReportDto>.Fail(
                    "invalid status transition", "INVALID_STATUS",
                    $"Report must be Collected to complete, current status: {report.Status}"));

            report.Status     = ReportStatus.Completed;
            report.CompleteAt = DateTime.UtcNow;
            report.UpdatedAt  = DateTime.UtcNow;
            await _reportRepository.UpdateAsync(report);

            return (200, ApiResponse<EnterpriseReportDto>.Success("report completed", MapToDto(report)));
        }

        // ── GET /enterprise/hubs/mine ─────────────────────────────────────────

        public async Task<(int, ApiResponse<StaffEnterpriseDto>)> GetMyEnterpriseAsync(
            string email, CancellationToken ct = default)
        {
            var (enterprise, staff) = await GetEnterpriseAndStaffAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<StaffEnterpriseDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));
            if (staff is null)
                return (403, ApiResponse<StaffEnterpriseDto>.Fail(
                    "forbidden", "FORBIDDEN", "You are not assigned as staff of any enterprise"));

            var dto = new StaffEnterpriseDto
            {
                EnterpriseId      = enterprise.Id,
                EnterpriseName    = enterprise.Name,
                EnterpriseEmail   = enterprise.Email,
                EnterpriseAddress = enterprise.Address,
                WorkAreaId        = enterprise.WorkAreaId,
                WorkAreaName      = enterprise.WorkArea?.Name,
                JoinHubAt         = staff?.JoinHubAt
            };

            return (200, ApiResponse<StaffEnterpriseDto>.Success("success", dto));
        }

        // ── CollectorHub CRUD ─────────────────────────────────────────────────

        public async Task<(int, ApiResponse<List<CollectorDtoNs.CollectorHubDto>>)> GetCollectorHubsAsync(
            string email, CancellationToken ct = default)
        {
            var (enterprise, _) = await GetEnterpriseAndStaffAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<List<CollectorDtoNs.CollectorHubDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            // Filter hubs to only those where the enterprise's collector staff work
            var collectors = await _collectorRepository.GetByEnterpriseIdAsync(enterprise.Id);
            var hubIds = new HashSet<Guid>();
            foreach (var c in collectors)
            {
                var staffList = await _collectorStaffRepository.GetByCollectorIdAsync(c.Id);
                foreach (var s in staffList)
                    if (s.CollectorHubId.HasValue) hubIds.Add(s.CollectorHubId.Value);
            }

            var allHubs = await _collectorHubRepository.GetAllAsync();
            var filtered = hubIds.Count > 0
                ? allHubs.Where(h => hubIds.Contains(h.Id)).ToList()
                : [];

            return (200, ApiResponse<List<CollectorDtoNs.CollectorHubDto>>.Success("success",
                filtered.Select(MapToCollectorHubDto).ToList()));
        }

        public async Task<(int, ApiResponse<CollectorDtoNs.CollectorHubDto>)> GetCollectorHubDetailAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var (enterprise, _) = await GetEnterpriseAndStaffAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDtoNs.CollectorHubDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var hub = await _collectorHubRepository.GetByIdAsync(id);
            if (hub is null)
                return (404, ApiResponse<CollectorDtoNs.CollectorHubDto>.Fail(
                    "not found", "NOT_FOUND", "Collector hub does not exist"));

            return (200, ApiResponse<CollectorDtoNs.CollectorHubDto>.Success("success", MapToCollectorHubDto(hub)));
        }

        public async Task<(int, ApiResponse<CollectorDtoNs.CollectorHubDto>)> CreateCollectorHubAsync(
            string email, CollectorDtoNs.SaveCollectorHubRequest request, CancellationToken ct = default)
        {
            var (enterprise, _) = await GetEnterpriseAndStaffAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDtoNs.CollectorHubDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var hub = new CollectorHub
            {
                Name             = request.Data.Name.Trim(),
                PhoneNumber      = request.Data.PhoneNumber.Trim(),
                Email            = request.Data.Email.Trim().ToLowerInvariant(),
                Address          = request.Data.Address.Trim(),
                Latitude         = request.Data.Latitude,
                Longitude        = request.Data.Longitude,
                WorkAreaId       = request.Data.WorkAreaId,
                AssignedCapacity = request.Data.AssignedCapacity
            };

            var created = await _collectorHubRepository.CreateAsync(hub);
            created = await _collectorHubRepository.GetByIdAsync(created.Id) ?? created;
            return (201, ApiResponse<CollectorDtoNs.CollectorHubDto>.Success("collector hub created", MapToCollectorHubDto(created)));
        }

        public async Task<(int, ApiResponse<CollectorDtoNs.CollectorHubDto>)> UpdateCollectorHubAsync(
            string email, Guid id, CollectorDtoNs.SaveCollectorHubRequest request, CancellationToken ct = default)
        {
            var (enterprise, _) = await GetEnterpriseAndStaffAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDtoNs.CollectorHubDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var hub = await _collectorHubRepository.GetByIdAsync(id);
            if (hub is null)
                return (404, ApiResponse<CollectorDtoNs.CollectorHubDto>.Fail(
                    "not found", "NOT_FOUND", "Collector hub does not exist"));

            hub.Name             = request.Data.Name.Trim();
            hub.PhoneNumber      = request.Data.PhoneNumber.Trim();
            hub.Email            = request.Data.Email.Trim().ToLowerInvariant();
            hub.Address          = request.Data.Address.Trim();
            if (request.Data.Latitude.HasValue)         hub.Latitude         = request.Data.Latitude;
            if (request.Data.Longitude.HasValue)        hub.Longitude        = request.Data.Longitude;
            if (request.Data.WorkAreaId.HasValue)       hub.WorkAreaId       = request.Data.WorkAreaId;
            if (request.Data.AssignedCapacity.HasValue) hub.AssignedCapacity = request.Data.AssignedCapacity;

            var updated = await _collectorHubRepository.UpdateAsync(hub);
            updated = await _collectorHubRepository.GetByIdAsync(updated.Id) ?? updated;
            return (200, ApiResponse<CollectorDtoNs.CollectorHubDto>.Success("collector hub updated", MapToCollectorHubDto(updated)));
        }

        public async Task<(int, ApiResponse<object>)> DeleteCollectorHubAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var (enterprise, _) = await GetEnterpriseAndStaffAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var hub = await _collectorHubRepository.GetByIdAsync(id);
            if (hub is null)
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Collector hub does not exist"));

            await _collectorHubRepository.DeleteAsync(hub);
            return (200, ApiResponse<object>.Success("collector hub deleted", null!));
        }

        // ── GET /enterprise/collectors ────────────────────────────────────────

        public async Task<(int, ApiResponse<List<CollectorDtoNs.CollectorDto>>)> GetCollectorsAsync(
            string email, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<List<CollectorDtoNs.CollectorDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collectors = await _collectorRepository.GetByEnterpriseIdAsync(enterprise.Id);
            return (200, ApiResponse<List<CollectorDtoNs.CollectorDto>>.Success("success",
                collectors.Select(MapToCollectorDto).ToList()));
        }

        // ── GET /enterprise/collectors/{id} ──────────────────────────────────

        public async Task<(int, ApiResponse<CollectorDtoNs.CollectorDto>)> GetCollectorDetailAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collector = await _collectorRepository.GetByIdAsync(id);
            if (collector is null || collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                    "not found", "NOT_FOUND", "Collector does not exist"));

            return (200, ApiResponse<CollectorDtoNs.CollectorDto>.Success("success", MapToCollectorDto(collector)));
        }

        // ── POST /enterprise/collectors ───────────────────────────────────────

        public async Task<(int, ApiResponse<CollectorDtoNs.CollectorDto>)> CreateCollectorAsync(
            string email, CollectorDtoNs.SaveCollectorRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            if (request.Data.WorkAreaId.HasValue)
            {
                var workArea = await _workAreaRepository.GetByIdAsync(request.Data.WorkAreaId.Value);
                if (workArea is null)
                    return (422, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                        "invalid work area", "INVALID_WORK_AREA", "Work area not found"));
                if (workArea.Type != "Ward")
                    return (422, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                        "invalid work area", "INVALID_WORK_AREA", "Collector work area must be a Ward"));

                if (enterprise.WorkAreaId.HasValue && workArea.ParentId != enterprise.WorkAreaId)
                    return (409, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                        "work area mismatch", "WORK_AREA_MISMATCH",
                        "This ward does not belong to the enterprise's district"));
            }

            var collector = new Collector
            {
                Name         = request.Data.Name.Trim(),
                PhoneNumber  = request.Data.PhoneNumber.Trim(),
                Email        = request.Data.Email.Trim().ToLowerInvariant(),
                Address      = request.Data.Address.Trim(),
                Latitude     = request.Data.Latitude,
                Longitude    = request.Data.Longitude,
                WorkAreaId   = request.Data.WorkAreaId,
                EnterpriseId = enterprise.Id
            };

            var created = await _collectorRepository.CreateAsync(collector);
            return (201, ApiResponse<CollectorDtoNs.CollectorDto>.Success("collector created", MapToCollectorDto(created)));
        }

        // ── PATCH /enterprise/collectors/{id} ────────────────────────────────

        public async Task<(int, ApiResponse<CollectorDtoNs.CollectorDto>)> UpdateCollectorAsync(
            string email, Guid id, CollectorDtoNs.SaveCollectorRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collector = await _collectorRepository.GetByIdAsync(id);
            if (collector is null || collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                    "not found", "NOT_FOUND", "Collector does not exist"));

            if (request.Data.WorkAreaId.HasValue)
            {
                var workArea = await _workAreaRepository.GetByIdAsync(request.Data.WorkAreaId.Value);
                if (workArea is null)
                    return (422, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                        "invalid work area", "INVALID_WORK_AREA", "Work area not found"));
                if (workArea.Type != "Ward")
                    return (422, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                        "invalid work area", "INVALID_WORK_AREA", "Collector work area must be a Ward"));

                if (enterprise.WorkAreaId.HasValue && workArea.ParentId != enterprise.WorkAreaId)
                    return (409, ApiResponse<CollectorDtoNs.CollectorDto>.Fail(
                        "work area mismatch", "WORK_AREA_MISMATCH",
                        "This ward does not belong to the enterprise's district"));
            }

            collector.Name       = request.Data.Name.Trim();
            collector.PhoneNumber = request.Data.PhoneNumber.Trim();
            collector.Email      = request.Data.Email.Trim().ToLowerInvariant();
            collector.Address    = request.Data.Address.Trim();
            if (request.Data.Latitude.HasValue)   collector.Latitude   = request.Data.Latitude;
            if (request.Data.Longitude.HasValue)  collector.Longitude  = request.Data.Longitude;
            if (request.Data.WorkAreaId.HasValue) collector.WorkAreaId = request.Data.WorkAreaId;

            var updated = await _collectorRepository.UpdateAsync(collector);
            return (200, ApiResponse<CollectorDtoNs.CollectorDto>.Success("collector updated", MapToCollectorDto(updated)));
        }

        // ── DELETE /enterprise/collectors/{id} ───────────────────────────────

        public async Task<(int, ApiResponse<object>)> DeleteCollectorAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var collector = await _collectorRepository.GetByIdAsync(id);
            if (collector is null || collector.EnterpriseId != enterprise.Id)
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Collector does not exist"));

            await _collectorRepository.DeleteAsync(collector);
            return (200, ApiResponse<object>.Success("collector deleted", null!));
        }

        // ── GET /enterprise/teams ─────────────────────────────────────────────

        public async Task<(int, ApiResponse<List<TeamDetailDto>>)> GetTeamsAsync(
            string email, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<List<TeamDetailDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var enterpriseTeamIds = await GetTeamIdsAsync(enterprise.Id);
            var teams             = await _teamRepository.GetByIdsAsync(enterpriseTeamIds);

            var memberCounts = new Dictionary<Guid, int>();
            foreach (var t in teams)
            {
                var s = await _collectorStaffRepository.GetByTeamIdAsync(t.Id);
                memberCounts[t.Id] = s.Count();
            }

            var dtos = teams.Select(t => MapToTeamDetailDto(t, memberCounts.GetValueOrDefault(t.Id, 0))).ToList();
            return (200, ApiResponse<List<TeamDetailDto>>.Success("success", dtos));
        }

        // ── GET /enterprise/teams/{id} ────────────────────────────────────────

        public async Task<(int, ApiResponse<TeamDetailDto>)> GetTeamDetailAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<TeamDetailDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team       = await _teamRepository.GetByIdAsync(id);
            var ownedTeams = await GetTeamIdsAsync(enterprise.Id);
            if (team is null || !ownedTeams.Contains(id))
                return (404, ApiResponse<TeamDetailDto>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            var staffs = await _collectorStaffRepository.GetByTeamIdAsync(id);
            return (200, ApiResponse<TeamDetailDto>.Success("success",
                MapToTeamDetailDto(team, staffs.Count())));
        }

        // ── POST /enterprise/teams ────────────────────────────────────────────

        public async Task<(int, ApiResponse<TeamDetailDto>)> CreateTeamAsync(
            string email, SaveTeamRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<TeamDetailDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var hub = await _collectorHubRepository.GetByIdAsync(request.Data.CollectorHubId);
            if (hub is null)
                return (422, ApiResponse<TeamDetailDto>.Fail(
                    "invalid collector hub", "INVALID_COLLECTOR_HUB",
                    "Collector hub does not exist"));

            var team = new Team
            {
                Name           = request.Data.Name.Trim(),
                CollectorHubId = request.Data.CollectorHubId,
                TotalCapacity  = request.Data.TotalCapacity,
                IsActive       = request.Data.IsActive,
                DispatchTime   = request.Data.DispatchTime?.Trim()
            };

            var created = await _teamRepository.CreateAsync(team);
            var reloaded = await _teamRepository.GetByIdAsync(created.Id);
            return (201, ApiResponse<TeamDetailDto>.Success("team created",
                MapToTeamDetailDto(reloaded!, 0)));
        }

        // ── PATCH /enterprise/teams/{id} ──────────────────────────────────────

        public async Task<(int, ApiResponse<TeamDetailDto>)> UpdateTeamAsync(
            string email, Guid id, SaveTeamRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<TeamDetailDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team        = await _teamRepository.GetByIdAsync(id);
            var ownedTeams2 = await GetTeamIdsAsync(enterprise.Id);
            if (team is null || !ownedTeams2.Contains(id))
                return (404, ApiResponse<TeamDetailDto>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            if (request.Data.CollectorHubId != team.CollectorHubId)
            {
                var newHub = await _collectorHubRepository.GetByIdAsync(request.Data.CollectorHubId);
                if (newHub is null)
                    return (422, ApiResponse<TeamDetailDto>.Fail(
                        "invalid collector hub", "INVALID_COLLECTOR_HUB",
                        "Collector hub does not exist"));
            }

            team.Name           = request.Data.Name.Trim();
            team.CollectorHubId = request.Data.CollectorHubId;
            team.TotalCapacity  = request.Data.TotalCapacity;
            team.IsActive       = request.Data.IsActive;
            team.DispatchTime   = request.Data.DispatchTime?.Trim();

            var updated = await _teamRepository.UpdateAsync(team);
            var staffs  = await _collectorStaffRepository.GetByTeamIdAsync(id);
            return (200, ApiResponse<TeamDetailDto>.Success("team updated",
                MapToTeamDetailDto(updated, staffs.Count())));
        }

        // ── DELETE /enterprise/teams/{id} ─────────────────────────────────────

        public async Task<(int, ApiResponse<object>)> DeleteTeamAsync(
            string email, Guid id, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team        = await _teamRepository.GetByIdAsync(id);
            var ownedTeams3 = await GetTeamIdsAsync(enterprise.Id);
            if (team is null || !ownedTeams3.Contains(id))
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            var staffs = await _collectorStaffRepository.GetByTeamIdAsync(id);
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
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<List<PointCategoryDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var categories = await _pointCategoryRepository.GetByEnterpriseIdAsync(enterprise.Id);
            return (200, ApiResponse<List<PointCategoryDto>>.Success("success",
                categories.Select(MapToCategoryDto).ToList()));
        }

        // ── POST /enterprise/point-categories ────────────────────────────────

        public async Task<(int, ApiResponse<PointCategoryDto>)> CreatePointCategoryAsync(
            string email, SavePointCategoryRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
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
            var enterprise = await GetEnterpriseAsync(email, ct);
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
            var enterprise = await GetEnterpriseAsync(email, ct);
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

        // ── GET /enterprise/teams/{teamId}/staff ──────────────────────────────

        public async Task<(int, ApiResponse<List<CollectorDtoNs.CollectorStaffDto>>)> GetTeamStaffAsync(
            string email, Guid teamId, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<List<CollectorDtoNs.CollectorStaffDto>>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team        = await _teamRepository.GetByIdAsync(teamId);
            var ownedTeams4 = await GetTeamIdsAsync(enterprise.Id);
            if (team is null || !ownedTeams4.Contains(teamId))
                return (404, ApiResponse<List<CollectorDtoNs.CollectorStaffDto>>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            var staffs = await _collectorStaffRepository.GetByTeamIdAsync(teamId);
            return (200, ApiResponse<List<CollectorDtoNs.CollectorStaffDto>>.Success("success",
                staffs.Select(MapToCollectorStaffDto).ToList()));
        }

        // ── POST /enterprise/teams/{teamId}/staff ─────────────────────────────

        public async Task<(int, ApiResponse<CollectorDtoNs.CollectorStaffDto>)> AddTeamStaffAsync(
            string email, Guid teamId, CollectorDtoNs.AddCollectorStaffRequest request, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<CollectorDtoNs.CollectorStaffDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var team        = await _teamRepository.GetByIdAsync(teamId);
            var ownedTeams5 = await GetTeamIdsAsync(enterprise.Id);
            if (team is null || !ownedTeams5.Contains(teamId))
                return (404, ApiResponse<CollectorDtoNs.CollectorStaffDto>.Fail(
                    "not found", "NOT_FOUND", "Team does not exist"));

            var staff = await _collectorStaffRepository.GetByUserIdAsync(request.Data.UserId);
            if (staff is null)
                return (404, ApiResponse<CollectorDtoNs.CollectorStaffDto>.Fail(
                    "staff not found", "NOT_FOUND",
                    "User must be set up as collector staff by admin first"));

            if (staff.Collector?.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<CollectorDtoNs.CollectorStaffDto>.Fail(
                    "forbidden", "FORBIDDEN",
                    "This staff member belongs to a different enterprise"));

            if (staff.TeamId.HasValue)
                return (409, ApiResponse<CollectorDtoNs.CollectorStaffDto>.Fail(
                    "already assigned to a team", "ALREADY_IN_TEAM",
                    "Remove staff from their current team first"));

            staff.TeamId     = teamId;
            staff.JoinTeamAt = DateTime.UtcNow;

            var updated  = await _collectorStaffRepository.UpdateAsync(staff);
            var reloaded = await _collectorStaffRepository.GetByUserIdAsync(updated.UserId);
            return (200, ApiResponse<CollectorDtoNs.CollectorStaffDto>.Success(
                "staff assigned to team", MapToCollectorStaffDto(reloaded!)));
        }

        // ── DELETE /enterprise/teams/{teamId}/staff/{userId} ──────────────────

        public async Task<(int, ApiResponse<object>)> RemoveTeamStaffAsync(
            string email, Guid teamId, Guid userId, CancellationToken ct = default)
        {
            var enterprise = await GetEnterpriseAsync(email, ct);
            if (enterprise is null)
                return (401, ApiResponse<object>.Fail(
                    "unauthorized", "UNAUTHORIZED", "Enterprise not found for this account"));

            var staff = await _collectorStaffRepository.GetByUserIdAsync(userId);
            if (staff is null || staff.TeamId != teamId)
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Staff member not found in this team"));

            if (staff.Collector?.EnterpriseId != enterprise.Id)
                return (403, ApiResponse<object>.Fail(
                    "forbidden", "FORBIDDEN", "Staff member does not belong to your enterprise"));

            var team        = await _teamRepository.GetByIdAsync(teamId);
            var ownedTeams6 = await GetTeamIdsAsync(enterprise.Id);
            if (team is null || !ownedTeams6.Contains(teamId))
                return (403, ApiResponse<object>.Fail(
                    "forbidden", "FORBIDDEN", "Team does not belong to your enterprise"));

            staff.TeamId     = null;
            staff.JoinTeamAt = null;
            await _collectorStaffRepository.UpdateAsync(staff);

            return (200, ApiResponse<object>.Success("staff removed from team", null!));
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

        private static CollectorDtoNs.CollectorDto MapToCollectorDto(Collector c) => new()
        {
            Id           = c.Id,
            Name         = c.Name,
            PhoneNumber  = c.PhoneNumber,
            Email        = c.Email,
            Address      = c.Address,
            Latitude     = c.Latitude,
            Longitude    = c.Longitude,
            WorkAreaId   = c.WorkAreaId,
            WorkAreaName = c.WorkArea?.Name,
            EnterpriseId = c.EnterpriseId,
            CreatedAt    = c.CreatedAt,
            UpdatedAt    = c.UpdatedAt
        };

        private static TeamDetailDto MapToTeamDetailDto(Team t, int memberCount) => new()
        {
            Id              = t.Id,
            Name            = t.Name,
            InWork          = t.InWork,
            IsActive        = t.IsActive,
            TotalCapacity   = t.TotalCapacity,
            CollectorHubId  = t.CollectorHubId,
            CollectorHubName = t.CollectorHub?.Name ?? string.Empty,
            DispatchTime    = t.DispatchTime,
            MemberCount     = memberCount,
            CreatedAt       = t.CreatedAt,
            UpdatedAt       = t.UpdatedAt
        };

        private static CollectorDtoNs.CollectorHubDto MapToCollectorHubDto(CollectorHub h) => new()
        {
            Id               = h.Id,
            Name             = h.Name,
            PhoneNumber      = h.PhoneNumber,
            Email            = h.Email,
            Address          = h.Address,
            Latitude         = h.Latitude,
            Longitude        = h.Longitude,
            WorkAreaId       = h.WorkAreaId,
            WorkAreaName     = h.WorkArea?.Name,
            AssignedCapacity = h.AssignedCapacity,
            CreatedAt        = h.CreatedAt,
            UpdatedAt        = h.UpdatedAt
        };

        private static CollectorDtoNs.CollectorStaffDto MapToCollectorStaffDto(CollectorStaff s) => new()
        {
            UserId         = s.UserId,
            UserEmail      = s.User?.Email      ?? string.Empty,
            UserFullName   = s.User?.FullName   ?? string.Empty,
            CollectorId    = s.CollectorId,
            CollectorHubId = s.CollectorHubId,
            TeamId         = s.TeamId,
            JoinTeamAt     = s.JoinTeamAt
        };
    }
}
