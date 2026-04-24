using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs.Complaint;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepository;
        private readonly ICitizenReportRepository _reportRepository;
        private readonly IUploadImageService _uploadImageService;

        public ComplaintService(
            IComplaintRepository complaintRepository,
            ICitizenReportRepository reportRepository,
            IUploadImageService uploadImageService)
        {
            _complaintRepository = complaintRepository;
            _reportRepository    = reportRepository;
            _uploadImageService   = uploadImageService;
        }

        public async Task<ComplaintResponseDto> CreateComplaintAsync(Guid citizenId, Guid reportId, CreateComplaintDto dto)
        {
            _ = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("report not found");

            var imageUrls = dto.Images.Count > 0
                ? await _uploadImageService.UploadImagesAsync(dto.Images, "complaints")
                : [];

            var complaint = new Complaint
            {
                CitizenId = citizenId,
                ReportId  = reportId,
                Reason    = dto.Reason,
                ImageUrls = imageUrls,
                Status    = ComplaintStatus.Pending,
                RequestAt = DateTime.UtcNow
            };

            await _complaintRepository.CreateAsync(complaint);
            var created = await _complaintRepository.GetByIdAsync(complaint.Id)
                ?? throw new InvalidOperationException("failed to load created complaint");
            return MapToResponse(created);
        }

        public async Task<ComplaintsListResult> GetComplaintsByReportAsync(Guid citizenId, Guid reportId, int page, int limit)
        {
            _ = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("report not found");

            var (items, total) = await _complaintRepository.GetByReportIdPagedAsync(reportId, page, limit);

            return new ComplaintsListResult
            {
                Complaints = items.Select(MapToResponse).ToList(),
                Pagination = new GarbageCollection.Common.DTOs.PaginationMeta
                {
                    Page       = page,
                    Limit      = limit,
                    Total      = total,
                    TotalPages = (int)Math.Ceiling((double)total / limit)
                }
            };
        }

        public async Task<ComplaintResponseDto> GetComplaintAsync(Guid citizenId, Guid reportId, Guid complaintId)
        {
            _ = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("report not found");

            var complaint = await _complaintRepository.GetByIdAsync(complaintId)
                ?? throw new KeyNotFoundException("complaint not found");

            if (complaint.CitizenId != citizenId)
                throw new UnauthorizedAccessException("forbidden");

            return MapToResponse(complaint);
        }

        public async Task SendMessageAsync(Guid citizenId, Guid reportId, Guid complaintId, string message, CancellationToken ct = default)
        {
            _ = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("report not found");

            var complaint = await _complaintRepository.GetByIdAsync(complaintId)
                ?? throw new KeyNotFoundException("complaint not found");

            if (complaint.ReportId != reportId)
                throw new KeyNotFoundException("complaint not found");

            if (complaint.CitizenId != citizenId)
                throw new UnauthorizedAccessException("forbidden");

            if (complaint.Status == ComplaintStatus.Approved || complaint.Status == ComplaintStatus.Rejected)
                throw new InvalidOperationException("complaint already resolved");

            var msg = new ComplaintMessage
            {
                Sender  = "citizen",
                Email   = complaint.Citizen?.Email ?? string.Empty,
                Message = message,
                Time    = DateTime.UtcNow
            };

            await _complaintRepository.AppendMessageAsync(complaintId, msg, ct);
        }

        private static ComplaintResponseDto MapToResponse(Complaint c) => new()
        {
            Id            = c.Id,
            CitizenId     = c.CitizenId,
            ReportId      = c.ReportId,
            Reason        = c.Reason,
            ImageUrls     = c.ImageUrls,
            Status        = c.Status.ToString(),
            AdminResponse = c.AdminResponse,
            RequestAt     = c.RequestAt,
            ResponseAt    = c.ResponseAt
        };
    }
}
