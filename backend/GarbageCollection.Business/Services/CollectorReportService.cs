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

        public CollectorReportService(
            IStaffRepository staffRepository,
            ITeamRepository teamRepository,
            ICollectorReportRepository reportRepository,
            ICitizenReportRepository citizenReportRepository,
            IPointCategoryRepository pointCategoryRepository,
            IUploadImageService uploadImageService)
        {
            _staffRepository           = staffRepository;
            _teamRepository            = teamRepository;
            _reportRepository          = reportRepository;
            _citizenReportRepository   = citizenReportRepository;
            _pointCategoryRepository   = pointCategoryRepository;
            _uploadImageService        = uploadImageService;
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
            if (await _reportRepository.HasOnTheWayTodayAsync(teamId, date))
                throw new InvalidOperationException("shift already started today");

            // B5: bulk update
            var updated = await _reportRepository.StartShiftAsync(teamId, date);

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

            // B4: status must be OnTheWay
            if (report.Status != ReportStatus.OnTheWay)
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
