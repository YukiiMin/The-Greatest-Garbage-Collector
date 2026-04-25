using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Complaint;
using GarbageCollection.Common.Enums;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Services
{
    public sealed class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IComplaintRepository _complaintRepository;
        private readonly ILogger<AdminService> _logger;

        // Allowed status values from the API spec (case-insensitive comparison).
        private static readonly IReadOnlySet<string> ValidStatuses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PENDING", "RESOLVED" };

        private const int MinLimit = 1;
        private const int MaxLimit = 100;
        private const int MinPage = 1;

        public AdminService(
            IUserRepository userRepository,
            IComplaintRepository complaintRepository,
            ILogger<AdminService> logger)
        {
            _userRepository = userRepository;
            _complaintRepository = complaintRepository;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────

        public async Task<GetComplaintsResult> GetComplaintsAsync(
            string tokenEmail,
            GetComplaintsRequestDto request,
            CancellationToken ct = default)
        {
            // ── STEP 2: Role check ────────────────────────────────────────────
            // We do not trust a role claim embedded in the JWT because role
            // changes would not invalidate existing tokens. The DB is the source
            // of truth. GetByEmailAsync uses AsNoTracking — read-only, fast.
            var user = await _userRepository.GetByEmailAsync(
                tokenEmail.Trim().ToLowerInvariant(), ct);

            if (user is null)
            {
                // The JWT was valid but no matching DB record — should not happen
                // in normal operation; treat as unauthorised.
                _logger.LogWarning(
                    "Admin complaints request from JWT email {Email} — user not found in DB.",
                    tokenEmail);

                return GetComplaintsResult.Failure(
                    statusCode: 401,
                    message: "unauthorized",
                    code: "UNAUTHORIZED",
                    description: "Invalid or missing access token.");
            }

            if(user.Role != UserRole.Admin) { 
                _logger.LogWarning(
                    "Non-admin user {Email} (role: {Role}) attempted to access admin complaints.",
                    tokenEmail, user.Role);

                return GetComplaintsResult.Failure(
                    statusCode: 403,
                    message: "forbidden",
                    code: "FORBIDDEN",
                    description: "User is not admin.");
            }     

            // ── STEP 3: Validate query parameters ─────────────────────────────
            if (!ValidStatuses.Contains(request.Status))
            {
                return GetComplaintsResult.Failure(
                    statusCode: 422,
                    message: "invalid query params",
                    code: "INVALID_QUERY",
                    description: $"status must be one of: {string.Join(", ", ValidStatuses)}.");
            }

            if (request.Page < MinPage)
            {
                return GetComplaintsResult.Failure(
                    statusCode: 422,
                    message: "invalid query params",
                    code: "INVALID_QUERY",
                    description: $"page must be >= {MinPage}.");
            }

            if (request.Limit < MinLimit || request.Limit > MaxLimit)
            {
                return GetComplaintsResult.Failure(
                    statusCode: 422,
                    message: "invalid query params",
                    code: "INVALID_QUERY",
                    description: $"limit must be between {MinLimit} and {MaxLimit}.");
            }

            // ── STEP 4: Map status string → enum ──────────────────────────────
            var statusEnum = Enum.Parse<ComplaintStatus>(request.Status, ignoreCase: true);

            // ── STEP 5 + 6: Query repository (list + count run concurrently) ──
            // Running both queries in parallel halves the DB round-trip overhead
            // at the cost of two concurrent connections from the pool.
         

            var complaints = await _complaintRepository.GetComplaintsAsync(
    statusEnum, request.Page, request.Limit, ct);

            var total = await _complaintRepository.CountAsync(
                statusEnum, ct);
            var complaintDtos = complaints.Select(c => new ComplaintItemDto
            {
                Id = c.Id,
                ReportId = c.ReportId,
                UserEmail = c.Citizen?.Email, 
                Title = c.Reason,           
                Status = c.Status.ToString().ToUpperInvariant(),
                CreatedAt = c.RequestAt      
            }).ToList();
            // ── STEP 8: Return success payload ────────────────────────────────
            return GetComplaintsResult.Ok(new ComplaintResponseDto
            {
                Complaints = complaintDtos,
                Pagination = new PaginationMeta
                {
                    Page = request.Page,
                    Limit = request.Limit,
                    Total = total
                }
            });
        }
        public async Task<(int, ApiResponse<ComplaintDetailResponseDto>)> GetComplaintDetailAsync(
    string email,
    Guid complaintId,
    CancellationToken ct = default)
        {
            // ── STEP 1: Check user ─────────────────────────────
            var user = await _userRepository.GetByEmailAsync(email, ct);

            if (user == null)
            {
                return (401, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "unauthorized",
                    "UNAUTHORIZED",
                    "User not found"));
            }

            if (user.Role != UserRole.Admin)
            {
                return (403, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "forbidden",
                    "FORBIDDEN",
                    "User is not admin"));
            }

            // ── STEP 2: Get complaint ─────────────────────────
            var complaint = await _complaintRepository.GetDetailAsync(complaintId, ct);

            if (complaint == null)
            {
                return (404, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "complaint not found",
                    "NOT_FOUND",
                    "Complaint does not exist"));
            }

            // ── STEP 3: Build timeline từ chính Complaint ─────
            var timeline = new List<AuditLogDto>();

            // CREATED
            timeline.Add(new AuditLogDto
            {
                Action = "CREATED",
                Actor = complaint.Citizen.Email,
                Timestamp = complaint.RequestAt,
                Note = "Complaint created"
            });

            // MESSAGES
            if (complaint.Messages != null && complaint.Messages.Any())
            {
                timeline.AddRange(complaint.Messages.Select(m => new AuditLogDto
                {
                    Action = "MESSAGE",
                    Actor = complaint.Citizen?.Email ?? "unknown",
                    Timestamp = m.Time
               
                }));
            }

            // RESPONSE (Admin xử lý)
            if (complaint.ResponseAt.HasValue)
            {
                timeline.Add(new AuditLogDto
                {
                    Action = complaint.Status.ToString().ToUpperInvariant(),
                    Actor = "admin",
                    Timestamp = complaint.ResponseAt.Value,
                    Note = complaint.AdminResponse
                });
            }

            // sort timeline
            timeline = timeline.OrderBy(x => x.Timestamp).ToList();

            // ── STEP 4: Map DTO ───────────────────────────────
            var response = new ComplaintDetailResponseDto
            {
                Complaint = new ComplaintDetailDto
                {
                    Id = complaint.Id,
                    Title = complaint.Reason,
                    Description = complaint.Reason,
                    ImageUrls = complaint.ImageUrls,
                    Status = complaint.Status.ToString().ToUpperInvariant(),
                    Messages = complaint.Messages
                },
                Report = new ReportDetailDto
                {
                    Id = complaint.Report.Id,
                    WasteCategories = complaint.Report.Types
    .Select(x => x.ToString().ToUpperInvariant())
    .ToList(),
                    CitizenImageUrls = complaint.Report.CitizenImageUrls,
                    CollectorImageUrls = complaint.Report.CollectorImageUrls,
                    Status = complaint.Report.Status.ToString().ToUpperInvariant(),
                    CollectedAt = complaint.Report.CollectedAt,
                    CitizenEmail = complaint.Report?.User?.Email
                },
                AuditTimeline = timeline
            };

            return (200, ApiResponse<ComplaintDetailResponseDto>.Success(
                "success",
                response));
        }
    }
    }
