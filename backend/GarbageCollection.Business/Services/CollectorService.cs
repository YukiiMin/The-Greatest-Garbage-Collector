using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Collector;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public sealed class CollectorService : ICollectorService
    {
        private readonly IUserRepository          _userRepository;
        private readonly ICollectorStaffRepository _collectorStaffRepository;

        public CollectorService(
            IUserRepository           userRepository,
            ICollectorStaffRepository collectorStaffRepository)
        {
            _userRepository           = userRepository;
            _collectorStaffRepository = collectorStaffRepository;
        }

        // ── Auth helper ───────────────────────────────────────────────────────

        private async Task<CollectorStaff?> GetCollectorStaffAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());
            if (user is null) return null;
            return await _collectorStaffRepository.GetByUserIdAsync(user.Id);
        }

        // ── GET /collector/staff ──────────────────────────────────────────────

        public async Task<(int, ApiResponse<List<CollectorStaffDto>>)> GetStaffAsync(
            string email, CancellationToken ct = default)
        {
            var staff = await GetCollectorStaffAsync(email);
            if (staff is null)
                return (403, ApiResponse<List<CollectorStaffDto>>.Fail(
                    "forbidden", "FORBIDDEN", "Collector staff record not found for this account"));

            var members = await _collectorStaffRepository.GetByCollectorIdAsync(staff.CollectorId);
            return (200, ApiResponse<List<CollectorStaffDto>>.Success("success",
                members.Select(MapToStaffDto).ToList()));
        }

        // ── POST /collector/staff ─────────────────────────────────────────────

        public async Task<(int, ApiResponse<CollectorStaffDto>)> AddStaffAsync(
            string email, AddCollectorStaffRequest request, CancellationToken ct = default)
        {
            var staff = await GetCollectorStaffAsync(email);
            if (staff is null)
                return (403, ApiResponse<CollectorStaffDto>.Fail(
                    "forbidden", "FORBIDDEN", "Collector staff record not found for this account"));

            var user = await _userRepository.GetByIdAsync(request.Data.UserId, ct);
            if (user is null)
                return (404, ApiResponse<CollectorStaffDto>.Fail(
                    "user not found", "NOT_FOUND", "User does not exist"));

            var existing = await _collectorStaffRepository.GetByUserIdAsync(request.Data.UserId);
            if (existing is not null)
                return (409, ApiResponse<CollectorStaffDto>.Fail(
                    "user already a collector staff", "STAFF_CONFLICT",
                    "This user already has a collector staff record"));

            // Đổi role → Collector và revoke sessions cũ
            var tracked = await _userRepository.GetByIdTrackedAsync(request.Data.UserId, ct)
                ?? throw new KeyNotFoundException("user not found");
            tracked.Role      = UserRole.Collector;
            tracked.LoginTerm++;
            tracked.UpdatedAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync(ct);

            var newStaff = new CollectorStaff
            {
                UserId      = request.Data.UserId,
                CollectorId = staff.CollectorId,
                TeamId      = null,
                JoinTeamAt  = null
            };

            var created = await _collectorStaffRepository.CreateAsync(newStaff);
            var reloaded = await _collectorStaffRepository.GetByUserIdAsync(created.UserId);
            return (201, ApiResponse<CollectorStaffDto>.Success("staff added", MapToStaffDto(reloaded!)));
        }

        // ── DELETE /collector/staff/{userId} ──────────────────────────────────

        public async Task<(int, ApiResponse<object>)> RemoveStaffAsync(
            string email, Guid userId, CancellationToken ct = default)
        {
            var staff = await GetCollectorStaffAsync(email);
            if (staff is null)
                return (403, ApiResponse<object>.Fail(
                    "forbidden", "FORBIDDEN", "Collector staff record not found for this account"));

            var target = await _collectorStaffRepository.GetByUserIdAsync(userId);
            if (target is null || target.CollectorId != staff.CollectorId)
                return (404, ApiResponse<object>.Fail(
                    "not found", "NOT_FOUND", "Staff member not found in your collector org"));

            await _collectorStaffRepository.DeleteAsync(target);

            // Reset role → Citizen và revoke sessions
            var user = await _userRepository.GetByIdTrackedAsync(userId, ct);
            if (user is not null)
            {
                user.Role      = UserRole.Citizen;
                user.LoginTerm++;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync(ct);
            }

            return (200, ApiResponse<object>.Success("staff removed", null!));
        }

        // ── Mappers ───────────────────────────────────────────────────────────

        private static CollectorStaffDto MapToStaffDto(CollectorStaff s) => new()
        {
            UserId         = s.UserId,
            UserEmail      = s.User?.Email ?? string.Empty,
            UserFullName   = s.User?.FullName ?? string.Empty,
            CollectorId    = s.CollectorId,
            CollectorHubId = s.CollectorHubId,
            TeamId         = s.TeamId,
            JoinTeamAt     = s.JoinTeamAt
        };
    }
}
