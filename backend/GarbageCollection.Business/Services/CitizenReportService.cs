using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.CitizenReport;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Exceptions;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class CitizenReportService : ICitizenReportService
    {
        private readonly ICitizenReportRepository _reportRepository;
        private readonly IUploadImageService _uploadImageService;

        public CitizenReportService(
            ICitizenReportRepository reportRepository,
            IUploadImageService uploadImageService)
        {
            _reportRepository = reportRepository;
            _uploadImageService = uploadImageService;
        }

        public async Task<CitizenReportResponseDto> CreateReportAsync(Guid userId, CreateCitizenReportDto dto)
        {
            if (dto.Types.Count > 4)
                throw new ArgumentException("Tối đa 4 loại rác mỗi báo cáo.");

            var imageUrls = await _uploadImageService.UploadImagesAsync(dto.Images, "citizen-reports");

            var report = new CitizenReport
            {
                UserId           = userId,
                CitizenImageUrls = imageUrls,
                Description      = dto.Description,
                Types            = dto.Types.ToList(),
                Capacity         = dto.Capacity,
                Status           = ReportStatus.Pending,
                ReportAt         = DateTime.UtcNow,
                CreatedAt        = DateTime.UtcNow
            };

            var created = await _reportRepository.CreateAsync(report);
            return MapToResponse(created);
        }

        public async Task<CitizenReportResponseDto?> GetReportByIdAsync(int id)
        {
            var report = await _reportRepository.GetByIdAsync(id);
            return report is null ? null : MapToResponse(report);
        }

        public async Task<IEnumerable<CitizenReportResponseDto>> GetReportsByUserAsync(Guid userId, ReportStatus? status = null)
        {
            var reports = await _reportRepository.GetByUserIdAsync(userId, status);
            return reports.Select(MapToResponse);
        }

        public async Task<CitizenReportsResult> GetUserReportsPagedAsync(Guid userId, int page, int limit)
        {
            var (items, total) = await _reportRepository.GetByUserIdPagedAsync(userId, page, limit);
            return new CitizenReportsResult
            {
                Reports = items.Select(MapToResponse).ToList(),
                Pagination = new PaginationMeta
                {
                    Page       = page,
                    Limit      = limit,
                    Total      = total,
                    TotalPages = (int)Math.Ceiling((double)total / limit)
                }
            };
        }

        public async Task CancelReportAsync(Guid userId, int reportId)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("report not found");

            if (report.UserId != userId)
                throw new UnauthorizedAccessException("you are not allowed to cancel this report");

            if (report.Status != ReportStatus.Pending)
                throw new InvalidOperationException("cannot cancel report that is not pending");

            if ((DateTime.UtcNow - report.CreatedAt).TotalMinutes > 10)
                throw new InvalidOperationException("cannot cancel report after 10 minutes");

            await _reportRepository.DeleteAsync(report);
        }

        public async Task<CitizenReportResponseDto> UpdateReportAsync(Guid userId, int reportId, UpdateCitizenReportDto dto)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("report not found");

            if (report.UserId != userId)
                throw new UnauthorizedAccessException("you are not allowed to update this report");

            if (report.Status != ReportStatus.Pending)
                throw new InvalidOperationException("cannot update report that is not pending");

            if (report.UpdatedAt.HasValue)
                throw new TooManyRequestsException("too many request");

            if (dto.Images != null && dto.Images.Count > 0)
            {
                await _uploadImageService.DeleteImagesAsync(report.CitizenImageUrls);
                report.CitizenImageUrls = await _uploadImageService.UploadImagesAsync(dto.Images, "citizen-reports");
            }

            if (dto.Types != null && dto.Types.Count > 0)
                report.Types = dto.Types.ToList();

            if (dto.Capacity.HasValue)
                report.Capacity = dto.Capacity;

            if (dto.Description != null)
                report.Description = dto.Description;

            var updated = await _reportRepository.UpdateAsync(report);
            return MapToResponse(updated);
        }

        public async Task<CitizenReportResponseDto> UpdateStatusAsync(int reportId, ReportStatus newStatus)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException($"Không tìm thấy báo cáo với ID {reportId}.");

            ValidateStatusTransition(report.Status, newStatus);

            report.Status = newStatus;

            if (newStatus == ReportStatus.Assigned)
                report.AssignAt = DateTime.UtcNow;
            else if (newStatus == ReportStatus.Processing)
                report.StartCollectingAt = DateTime.UtcNow;
            else if (newStatus == ReportStatus.Collected)
                report.CollectedAt = DateTime.UtcNow;
            else if (newStatus == ReportStatus.Completed)
                report.CompleteAt = DateTime.UtcNow;

            var updated = await _reportRepository.UpdateAsync(report);
            return MapToResponse(updated);
        }

        private static void ValidateStatusTransition(ReportStatus current, ReportStatus next)
        {
            var allowed = new Dictionary<ReportStatus, ReportStatus[]>
            {
                { ReportStatus.Pending,           [ReportStatus.Queue,             ReportStatus.Rejected, ReportStatus.Cancel] },
                { ReportStatus.Queue,             [ReportStatus.QueuedForDispatch, ReportStatus.Rejected, ReportStatus.Cancel] },
                { ReportStatus.QueuedForDispatch, [ReportStatus.OnTheWay,          ReportStatus.Rejected, ReportStatus.Cancel] },
                { ReportStatus.OnTheWay,          [ReportStatus.Assigned,          ReportStatus.Failed,   ReportStatus.Cancel] },
                { ReportStatus.Assigned,          [ReportStatus.Processing,        ReportStatus.Failed,   ReportStatus.Cancel] },
                { ReportStatus.Processing,        [ReportStatus.Collected,         ReportStatus.Failed,   ReportStatus.Cancel] },
                { ReportStatus.Collected,         [ReportStatus.Completed]                                                     },
            };

            if (!allowed.TryGetValue(current, out var allowedNext) || !allowedNext.Contains(next))
            {
                var valid = allowed.ContainsKey(current)
                    ? string.Join(", ", allowed[current])
                    : "không có";
                throw new InvalidOperationException(
                    $"Không thể chuyển trạng thái từ '{current}' sang '{next}'. Trạng thái hợp lệ tiếp theo: {valid}.");
            }
        }

        private static CitizenReportResponseDto MapToResponse(CitizenReport report) => new()
        {
            Id                 = report.Id,
            CitizenImageUrls   = report.CitizenImageUrls,
            Types              = report.Types.Select(t => t.ToString()).ToList(),
            Capacity           = report.Capacity,
            Description        = report.Description,
            Status             = report.Status.ToString(),
            UserId             = report.UserId,
            PointCategoryId    = report.PointCategoryId,
            Point              = report.Point,
            TeamId             = report.TeamId,
            ReportNote         = report.ReportNote,
            AssignAt           = report.AssignAt,
            StartCollectingAt  = report.StartCollectingAt,
            CollectedAt        = report.CollectedAt,
            ReportAt           = report.ReportAt,
            CollectorImageUrls = report.CollectorImageUrls,
            CompleteAt         = report.CompleteAt,
            CreatedAt          = report.CreatedAt,
            UpdatedAt          = report.UpdatedAt,
        };
    }
}
