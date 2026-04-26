using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Admin;
using GarbageCollection.Common.DTOs.Complaint;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;

namespace GarbageCollection.Business.Services
{
    public sealed class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IComplaintRepository _complaintRepository;
        private readonly IEnterpriseRepository _enterpriseRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly ILogger<AdminService> _logger;

        // Valid statuses for listing: maps to ComplaintStatus enum values
        private static readonly IReadOnlySet<string> ValidListStatuses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "PENDING", "APPROVED", "REJECTED" };

        // Valid statuses for resolving a complaint
        private static readonly IReadOnlySet<string> ValidResolveStatuses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "APPROVED", "REJECTED" };

        private const int MinLimit = 1;
        private const int MaxLimit = 100;
        private const int MinPage  = 1;

        public AdminService(
            IUserRepository userRepository,
            IComplaintRepository complaintRepository,
            IEnterpriseRepository enterpriseRepository,
            IStaffRepository staffRepository,
            ILogger<AdminService> logger)
        {
            _userRepository      = userRepository;
            _complaintRepository = complaintRepository;
            _enterpriseRepository = enterpriseRepository;
            _staffRepository     = staffRepository;
            _logger              = logger;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task<(UserRole? role, bool found)> GetRoleAsync(
            string email, CancellationToken ct)
        {
            var user = await _userRepository.GetByEmailAsync(
                email.Trim().ToLowerInvariant(), ct);
            return user is null ? (null, false) : (user.Role, true);
        }

        // ── GET /admin/complaints ─────────────────────────────────────────────

        public async Task<GetComplaintsResult> GetComplaintsAsync(
            string tokenEmail,
            GetComplaintsRequestDto request,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetByEmailAsync(
                tokenEmail.Trim().ToLowerInvariant(), ct);

            if (user is null)
            {
                _logger.LogWarning(
                    "Admin complaints request from JWT email {Email} — user not found in DB.",
                    tokenEmail);
                return GetComplaintsResult.Failure(
                    statusCode: 401,
                    message: "unauthorized",
                    code: "UNAUTHORIZED",
                    description: "Invalid or missing access token.");
            }

            if (user.Role != UserRole.Admin)
            {
                _logger.LogWarning(
                    "Non-admin user {Email} (role: {Role}) attempted to access admin complaints.",
                    tokenEmail, user.Role);
                return GetComplaintsResult.Failure(
                    statusCode: 403,
                    message: "forbidden",
                    code: "FORBIDDEN",
                    description: "User is not admin.");
            }

            if (!ValidListStatuses.Contains(request.Status))
            {
                return GetComplaintsResult.Failure(
                    statusCode: 422,
                    message: "invalid query params",
                    code: "INVALID_QUERY",
                    description: $"status must be one of: {string.Join(", ", ValidListStatuses)}.");
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

            var statusEnum = Enum.Parse<ComplaintStatus>(request.Status, ignoreCase: true);

            var complaints = await _complaintRepository.GetComplaintsAsync(
                statusEnum, request.Page, request.Limit, ct);

            var total = await _complaintRepository.CountAsync(statusEnum, ct);

            var complaintDtos = complaints.Select(c => new ComplaintItemDto
            {
                Id        = c.Id,
                ReportId  = c.ReportId,
                UserEmail = c.Citizen?.Email ?? string.Empty,
                Title     = c.Reason,
                Status    = c.Status.ToString().ToUpperInvariant(),
                CreatedAt = c.RequestAt
            }).ToList();

            return GetComplaintsResult.Ok(new ComplaintResponseDto
            {
                Complaints = complaintDtos,
                Pagination = new PaginationMeta
                {
                    Page  = request.Page,
                    Limit = request.Limit,
                    Total = total
                }
            });
        }

        // ── GET /admin/complaints/{id} ────────────────────────────────────────

        public async Task<(int, ApiResponse<ComplaintDetailResponseDto>)> GetComplaintDetailAsync(
            string email,
            Guid complaintId,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, ct);
            if (user is null)
                return (401, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "User not found"));

            if (user.Role != UserRole.Admin)
                return (403, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "forbidden", "FORBIDDEN", "User is not admin"));

            var complaint = await _complaintRepository.GetDetailAsync(complaintId, ct);
            if (complaint is null)
                return (404, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "complaint not found", "NOT_FOUND", "Complaint does not exist"));

            return (200, ApiResponse<ComplaintDetailResponseDto>.Success(
                "success",
                BuildDetailResponse(complaint)));
        }

        // ── PATCH /admin/complaints/{id} ──────────────────────────────────────

        public async Task<(int, ApiResponse<ComplaintDetailResponseDto>)> ResolveComplaintAsync(
            string email,
            Guid complaintId,
            ResolveComplaintRequest request,
            CancellationToken ct = default)
        {
            // Auth
            var user = await _userRepository.GetByEmailAsync(email, ct);
            if (user is null)
                return (401, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "User not found"));

            if (user.Role != UserRole.Admin)
                return (403, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "forbidden", "FORBIDDEN", "User is not admin"));

            // Validate status
            if (!ValidResolveStatuses.Contains(request.Data.Status))
                return (422, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "invalid status", "INVALID_STATUS",
                    $"status must be one of: {string.Join(", ", ValidResolveStatuses)}"));

            if (string.IsNullOrWhiteSpace(request.Data.AdminResponse))
                return (422, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "admin_response is required", "MISSING_RESPONSE",
                    "admin_response cannot be empty"));

            // Get complaint
            var complaint = await _complaintRepository.GetDetailAsync(complaintId, ct);
            if (complaint is null)
                return (404, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "complaint not found", "NOT_FOUND", "Complaint does not exist"));

            // Must be Pending to be resolved
            if (complaint.Status != ComplaintStatus.Pending)
                return (409, ApiResponse<ComplaintDetailResponseDto>.Fail(
                    "complaint already resolved", "ALREADY_RESOLVED",
                    $"Complaint is already {complaint.Status}"));

            // Apply
            complaint.Status        = Enum.Parse<ComplaintStatus>(request.Data.Status, ignoreCase: true);
            complaint.AdminResponse = request.Data.AdminResponse;
            complaint.ResponseAt    = DateTime.UtcNow;

            await _complaintRepository.UpdateAsync(complaint);

            return (200, ApiResponse<ComplaintDetailResponseDto>.Success(
                "complaint resolved",
                BuildDetailResponse(complaint)));
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static ComplaintDetailResponseDto BuildDetailResponse(
            Common.Models.Complaint complaint)
        {
            var timeline = new List<AuditLogDto>
            {
                new()
                {
                    Action    = "CREATED",
                    Actor     = complaint.Citizen?.Email ?? "unknown",
                    Timestamp = complaint.RequestAt,
                    Note      = "Complaint created"
                }
            };

            if (complaint.Messages is { Count: > 0 })
            {
                timeline.AddRange(complaint.Messages.Select(m => new AuditLogDto
                {
                    Action    = "MESSAGE",
                    Actor     = complaint.Citizen?.Email ?? "unknown",
                    Timestamp = m.Time
                }));
            }

            if (complaint.ResponseAt.HasValue)
            {
                timeline.Add(new AuditLogDto
                {
                    Action    = complaint.Status.ToString().ToUpperInvariant(),
                    Actor     = "admin",
                    Timestamp = complaint.ResponseAt.Value,
                    Note      = complaint.AdminResponse
                });
            }

            timeline = [.. timeline.OrderBy(x => x.Timestamp)];

            return new ComplaintDetailResponseDto
            {
                Complaint = new ComplaintDetailDto
                {
                    Id        = complaint.Id,
                    Title     = complaint.Reason,
                    Description = complaint.Reason,
                    ImageUrls = complaint.ImageUrls,
                    Status    = complaint.Status.ToString().ToUpperInvariant(),
                    Messages  = complaint.Messages
                },
                Report = new ReportDetailDto
                {
                    Id                 = complaint.Report.Id,
                    WasteCategories    = complaint.Report.Types
                        .Select(x => x.ToString().ToUpperInvariant()).ToList(),
                    CitizenImageUrls   = complaint.Report.CitizenImageUrls,
                    CollectorImageUrls = complaint.Report.CollectorImageUrls,
                    Status             = complaint.Report.Status.ToString().ToUpperInvariant(),
                    CollectedAt        = complaint.Report.CollectedAt,
                    CitizenEmail       = complaint.Report?.User?.Email ?? string.Empty
                },
                AuditTimeline = timeline
            };
        }

        // ── GET /admin/users ──────────────────────────────────────────────────

        public async Task<(int, ApiResponse<AdminUserListResponseDto>)> GetUsersAsync(
            string email,
            string? search,
            string? role,
            bool? isBanned,
            int page,
            int limit,
            CancellationToken ct = default)
        {
            var caller = await _userRepository.GetByEmailAsync(email, ct);
            if (caller is null)
                return (401, ApiResponse<AdminUserListResponseDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "User not found"));
            if (caller.Role != UserRole.Admin)
                return (403, ApiResponse<AdminUserListResponseDto>.Fail(
                    "forbidden", "FORBIDDEN", "User is not admin"));

            // Validate role filter
            UserRole? roleFilter = null;
            if (!string.IsNullOrWhiteSpace(role))
            {
                if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed))
                    return (422, ApiResponse<AdminUserListResponseDto>.Fail(
                        "invalid role", "INVALID_ROLE",
                        "role must be one of: Citizen, Collector, Enterprise, Admin"));
                roleFilter = parsed;
            }

            if (page < 1)
                return (422, ApiResponse<AdminUserListResponseDto>.Fail(
                    "invalid page", "INVALID_QUERY", "page must be >= 1"));
            if (limit < 1 || limit > 100)
                return (422, ApiResponse<AdminUserListResponseDto>.Fail(
                    "invalid limit", "INVALID_QUERY", "limit must be 1–100"));

            var (users, total) = await _userRepository.GetPagedAsync(
                search, roleFilter, isBanned, page, limit, ct);

            var dtos = users.Select(MapToAdminUserDto).ToList();

            return (200, ApiResponse<AdminUserListResponseDto>.Success("success",
                new AdminUserListResponseDto
                {
                    Users      = dtos,
                    Pagination = new PaginationMeta { Page = page, Limit = limit, Total = total }
                }));
        }

        // ── PATCH /admin/users/{id}/role ──────────────────────────────────────

        public async Task<(int, ApiResponse<AdminUserDto>)> ChangeRoleAsync(
            string email,
            Guid targetUserId,
            ChangeRoleRequest request,
            CancellationToken ct = default)
        {
            var caller = await _userRepository.GetByEmailAsync(email, ct);
            if (caller is null)
                return (401, ApiResponse<AdminUserDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "User not found"));
            if (caller.Role != UserRole.Admin)
                return (403, ApiResponse<AdminUserDto>.Fail(
                    "forbidden", "FORBIDDEN", "User is not admin"));

            if (!Enum.TryParse<UserRole>(request.Data.Role, ignoreCase: true, out var newRole))
                return (422, ApiResponse<AdminUserDto>.Fail(
                    "invalid role", "INVALID_ROLE",
                    "role must be one of: Citizen, Collector, Enterprise, Admin"));

            var target = await _userRepository.GetByIdTrackedAsync(targetUserId, ct);
            if (target is null)
                return (404, ApiResponse<AdminUserDto>.Fail(
                    "user not found", "NOT_FOUND", "User does not exist"));

            // Prevent admin from demoting themselves
            if (target.Id == caller.Id && newRole != UserRole.Admin)
                return (409, ApiResponse<AdminUserDto>.Fail(
                    "cannot demote yourself", "SELF_DEMOTION",
                    "Admin cannot change their own role"));

            target.Role      = newRole;
            target.UpdatedAt = DateTime.UtcNow;
            // Revoke all sessions so new role takes effect on next login
            target.LoginTerm++;

            await _userRepository.UpdateAsync(target, ct);

            return (200, ApiResponse<AdminUserDto>.Success("role updated", MapToAdminUserDto(target)));
        }

        // ── PATCH /admin/users/{id}/ban ───────────────────────────────────────

        public async Task<(int, ApiResponse<AdminUserDto>)> BanUserAsync(
            string email,
            Guid targetUserId,
            BanUserRequest request,
            CancellationToken ct = default)
        {
            var caller = await _userRepository.GetByEmailAsync(email, ct);
            if (caller is null)
                return (401, ApiResponse<AdminUserDto>.Fail(
                    "unauthorized", "UNAUTHORIZED", "User not found"));
            if (caller.Role != UserRole.Admin)
                return (403, ApiResponse<AdminUserDto>.Fail(
                    "forbidden", "FORBIDDEN", "User is not admin"));

            var target = await _userRepository.GetByIdTrackedAsync(targetUserId, ct);
            if (target is null)
                return (404, ApiResponse<AdminUserDto>.Fail(
                    "user not found", "NOT_FOUND", "User does not exist"));

            if (target.Id == caller.Id)
                return (409, ApiResponse<AdminUserDto>.Fail(
                    "cannot ban yourself", "SELF_BAN",
                    "Admin cannot ban their own account"));

            target.IsBanned  = request.Data.IsBanned;
            target.UpdatedAt = DateTime.UtcNow;
            if (request.Data.IsBanned)
                target.LoginTerm++; // invalidate existing sessions immediately

            await _userRepository.UpdateAsync(target, ct);

            var msg = request.Data.IsBanned ? "user banned" : "user unbanned";
            return (200, ApiResponse<AdminUserDto>.Success(msg, MapToAdminUserDto(target)));
        }

        // ── Enterprise CRUD ───────────────────────────────────────────────────

        public async Task<(int, ApiResponse<List<AdminEnterpriseDto>>)> GetEnterprisesAsync(
            string adminEmail, CancellationToken ct)
        {
            var admin = await _userRepository.GetByEmailAsync(adminEmail, ct);
            if (admin is null)
                return (401, ApiResponse<List<AdminEnterpriseDto>>.Fail("unauthorized", "UNAUTHORIZED"));
            if (admin.Role != UserRole.Admin)
                return (403, ApiResponse<List<AdminEnterpriseDto>>.Fail("forbidden", "FORBIDDEN"));

            var enterprises = await _enterpriseRepository.GetAllAsync();
            var dtos = enterprises.Select(MapToEnterpriseDto).ToList();
            return (200, ApiResponse<List<AdminEnterpriseDto>>.Success("success", dtos));
        }

        public async Task<(int, ApiResponse<AdminEnterpriseDto>)> GetEnterpriseDetailAsync(
            string adminEmail, Guid id, CancellationToken ct)
        {
            var admin = await _userRepository.GetByEmailAsync(adminEmail, ct);
            if (admin is null)
                return (401, ApiResponse<AdminEnterpriseDto>.Fail("unauthorized", "UNAUTHORIZED"));
            if (admin.Role != UserRole.Admin)
                return (403, ApiResponse<AdminEnterpriseDto>.Fail("forbidden", "FORBIDDEN"));

            var enterprise = await _enterpriseRepository.GetByIdAsync(id);
            if (enterprise is null)
                return (404, ApiResponse<AdminEnterpriseDto>.Fail("enterprise not found", "NOT_FOUND"));

            return (200, ApiResponse<AdminEnterpriseDto>.Success("success", MapToEnterpriseDto(enterprise)));
        }

        public async Task<(int, ApiResponse<AdminEnterpriseDto>)> CreateEnterpriseAsync(
            string adminEmail, SaveAdminEnterpriseRequest req, CancellationToken ct)
        {
            var admin = await _userRepository.GetByEmailAsync(adminEmail, ct);
            if (admin is null)
                return (401, ApiResponse<AdminEnterpriseDto>.Fail("unauthorized", "UNAUTHORIZED"));
            if (admin.Role != UserRole.Admin)
                return (403, ApiResponse<AdminEnterpriseDto>.Fail("forbidden", "FORBIDDEN"));

            var existing = await _enterpriseRepository.GetByEmailAsync(req.Data.Email);
            if (existing is not null)
                return (409, ApiResponse<AdminEnterpriseDto>.Fail(
                    "email already used", "ENTERPRISE_EMAIL_CONFLICT",
                    "An enterprise with that email already exists"));

            var enterprise = new Enterprise
            {
                Name        = req.Data.Name,
                PhoneNumber = req.Data.PhoneNumber,
                Email       = req.Data.Email,
                Address     = req.Data.Address,
                WorkAreaId  = req.Data.WorkAreaId,
                Latitude    = req.Data.Latitude,
                Longitude   = req.Data.Longitude,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow
            };

            var created = await _enterpriseRepository.CreateAsync(enterprise);
            return (201, ApiResponse<AdminEnterpriseDto>.Success("enterprise created", MapToEnterpriseDto(created)));
        }

        public async Task<(int, ApiResponse<AdminEnterpriseDto>)> UpdateEnterpriseAsync(
            string adminEmail, Guid id, SaveAdminEnterpriseRequest req, CancellationToken ct)
        {
            var admin = await _userRepository.GetByEmailAsync(adminEmail, ct);
            if (admin is null)
                return (401, ApiResponse<AdminEnterpriseDto>.Fail("unauthorized", "UNAUTHORIZED"));
            if (admin.Role != UserRole.Admin)
                return (403, ApiResponse<AdminEnterpriseDto>.Fail("forbidden", "FORBIDDEN"));

            var enterprise = await _enterpriseRepository.GetByIdAsync(id);
            if (enterprise is null)
                return (404, ApiResponse<AdminEnterpriseDto>.Fail("enterprise not found", "NOT_FOUND"));

            enterprise.Name        = req.Data.Name;
            enterprise.PhoneNumber = req.Data.PhoneNumber;
            enterprise.Email       = req.Data.Email;
            enterprise.Address     = req.Data.Address;
            if (req.Data.WorkAreaId.HasValue)  enterprise.WorkAreaId = req.Data.WorkAreaId;
            if (req.Data.Latitude.HasValue)    enterprise.Latitude   = req.Data.Latitude;
            if (req.Data.Longitude.HasValue)   enterprise.Longitude  = req.Data.Longitude;
            enterprise.UpdatedAt   = DateTime.UtcNow;

            var updated = await _enterpriseRepository.UpdateAsync(enterprise);
            return (200, ApiResponse<AdminEnterpriseDto>.Success("enterprise updated", MapToEnterpriseDto(updated)));
        }

        public async Task<(int, ApiResponse<object>)> DeleteEnterpriseAsync(
            string adminEmail, Guid id, CancellationToken ct)
        {
            var admin = await _userRepository.GetByEmailAsync(adminEmail, ct);
            if (admin is null)
                return (401, ApiResponse<object>.Fail("unauthorized", "UNAUTHORIZED"));
            if (admin.Role != UserRole.Admin)
                return (403, ApiResponse<object>.Fail("forbidden", "FORBIDDEN"));

            var enterprise = await _enterpriseRepository.GetByIdAsync(id);
            if (enterprise is null)
                return (404, ApiResponse<object>.Fail("enterprise not found", "NOT_FOUND"));

            var staffs = await _staffRepository.GetByEnterpriseIdAsync(id);
            if (staffs.Any())
                return (409, ApiResponse<object>.Fail(
                    "enterprise has staff", "ENTERPRISE_HAS_STAFF",
                    "Remove all staff before deleting the enterprise"));

            await _enterpriseRepository.DeleteAsync(enterprise);
            return (200, ApiResponse<object>.Success("enterprise deleted", null!));
        }

        // ── Setup accounts ────────────────────────────────────────────────────

        public async Task<(int, ApiResponse<AdminEnterpriseDto>)> SetupEnterpriseUserAsync(
            string adminEmail, AdminSetupEnterpriseRequest req, CancellationToken ct)
        {
            var admin = await _userRepository.GetByEmailAsync(adminEmail, ct);
            if (admin is null)
                return (401, ApiResponse<AdminEnterpriseDto>.Fail("unauthorized", "UNAUTHORIZED"));
            if (admin.Role != UserRole.Admin)
                return (403, ApiResponse<AdminEnterpriseDto>.Fail("forbidden", "FORBIDDEN"));

            var target = await _userRepository.GetByIdAsync(req.Data.UserId, ct);
            if (target is null)
                return (404, ApiResponse<AdminEnterpriseDto>.Fail("user not found", "NOT_FOUND"));

            var existingEnterprise = await _enterpriseRepository.GetByEmailAsync(target.Email);
            if (existingEnterprise is not null)
                return (409, ApiResponse<AdminEnterpriseDto>.Fail(
                    "enterprise already exists for this user", "ENTERPRISE_CONFLICT",
                    "An enterprise is already linked to this user's email"));

            var enterprise = new Enterprise
            {
                Name        = req.Data.Name,
                PhoneNumber = req.Data.PhoneNumber,
                Email       = target.Email,
                Address     = req.Data.Address,
                WorkAreaId  = req.Data.WorkAreaId,
                Latitude    = req.Data.Latitude,
                Longitude   = req.Data.Longitude,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow
            };
            var created = await _enterpriseRepository.CreateAsync(enterprise);

            return (201, ApiResponse<AdminEnterpriseDto>.Success("enterprise hub created", MapToEnterpriseDto(created)));
        }

        public async Task<(int, ApiResponse<AdminSetupResponseDto>)> AssignEnterpriseUserAsync(
            string adminEmail, Guid enterpriseId, AssignEnterpriseRequest req, CancellationToken ct)
        {
            var admin = await _userRepository.GetByEmailAsync(adminEmail, ct);
            if (admin is null)
                return (401, ApiResponse<AdminSetupResponseDto>.Fail("unauthorized", "UNAUTHORIZED"));
            if (admin.Role != UserRole.Admin)
                return (403, ApiResponse<AdminSetupResponseDto>.Fail("forbidden", "FORBIDDEN"));

            var enterprise = await _enterpriseRepository.GetByIdAsync(enterpriseId);
            if (enterprise is null)
                return (404, ApiResponse<AdminSetupResponseDto>.Fail("enterprise not found", "NOT_FOUND"));

            var target = await _userRepository.GetByIdTrackedAsync(req.Data.UserId, ct);
            if (target is null)
                return (404, ApiResponse<AdminSetupResponseDto>.Fail("user not found", "NOT_FOUND"));

            var existingStaff = await _staffRepository.GetByUserIdAsync(req.Data.UserId);
            if (existingStaff is not null)
                return (409, ApiResponse<AdminSetupResponseDto>.Fail(
                    "user is already a staff member", "STAFF_CONFLICT",
                    "This user already has a staff record"));

            var staff = new Staff
            {
                UserId       = req.Data.UserId,
                EnterpriseId = enterpriseId,
                CollectorId  = null,
                TeamId       = null,
                JoinTeamAt   = null
            };
            await _staffRepository.CreateAsync(staff);

            target.Role      = UserRole.Enterprise;
            target.LoginTerm++;
            target.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(target, ct);

            return (200, ApiResponse<AdminSetupResponseDto>.Success("enterprise assigned", new AdminSetupResponseDto
            {
                User      = MapToAdminUserDto(target),
                ExtraData = MapToEnterpriseDto(enterprise)
            }));
        }

        public async Task<(int, ApiResponse<AdminSetupResponseDto>)> SetupCollectorUserAsync(
            string adminEmail, AdminSetupCollectorRequest req, CancellationToken ct)
        {
            var admin = await _userRepository.GetByEmailAsync(adminEmail, ct);
            if (admin is null)
                return (401, ApiResponse<AdminSetupResponseDto>.Fail("unauthorized", "UNAUTHORIZED"));
            if (admin.Role != UserRole.Admin)
                return (403, ApiResponse<AdminSetupResponseDto>.Fail("forbidden", "FORBIDDEN"));

            var target = await _userRepository.GetByIdTrackedAsync(req.Data.UserId, ct);
            if (target is null)
                return (404, ApiResponse<AdminSetupResponseDto>.Fail("user not found", "NOT_FOUND"));

            var enterprise = await _enterpriseRepository.GetByIdAsync(req.Data.EnterpriseId);
            if (enterprise is null)
                return (404, ApiResponse<AdminSetupResponseDto>.Fail("enterprise not found", "NOT_FOUND", "Enterprise does not exist"));

            var existingStaff = await _staffRepository.GetByUserIdAsync(req.Data.UserId);
            if (existingStaff is not null)
                return (409, ApiResponse<AdminSetupResponseDto>.Fail(
                    "user is already a staff member", "STAFF_CONFLICT",
                    "This user already has a staff record"));

            var staff = new Staff
            {
                UserId       = req.Data.UserId,
                EnterpriseId = req.Data.EnterpriseId,
                CollectorId  = null,
                TeamId       = null,
                JoinTeamAt   = null
            };
            await _staffRepository.CreateAsync(staff);

            target.Role      = UserRole.Collector;
            target.LoginTerm++;
            target.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(target, ct);

            return (201, ApiResponse<AdminSetupResponseDto>.Success("collector user set up", new AdminSetupResponseDto
            {
                User      = MapToAdminUserDto(target),
                ExtraData = null
            }));
        }

        // ── DTO mappers ───────────────────────────────────────────────────────

        private static AdminUserDto MapToAdminUserDto(Common.Models.User u) => new()
        {
            Id            = u.Id,
            Email         = u.Email,
            FullName      = u.FullName,
            Role          = u.Role.ToString(),
            IsBanned      = u.IsBanned,
            EmailVerified = u.EmailVerified,
            Provider      = u.Provider,
            AvatarUrl     = u.AvatarUrl,
            CreatedAt     = u.CreatedAt
        };

        private static AdminEnterpriseDto MapToEnterpriseDto(Enterprise e) => new()
        {
            Id           = e.Id,
            Name         = e.Name,
            PhoneNumber  = e.PhoneNumber,
            Email        = e.Email,
            Address      = e.Address,
            WorkAreaId   = e.WorkAreaId,
            WorkAreaName = e.WorkArea?.Name,
            Latitude     = e.Latitude,
            Longitude    = e.Longitude,
            CreatedAt    = e.CreatedAt,
            UpdatedAt    = e.UpdatedAt
        };
    }
}
