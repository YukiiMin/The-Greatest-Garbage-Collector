using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs.User;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository         _userRepository;
        private readonly IWorkAreaRepository     _workAreaRepository;
        private readonly IUserPointsRepository   _userPointsRepository;
        private readonly JwtHelper _jwtHelper;

        public UserService(
            IUserRepository userRepository,
            IWorkAreaRepository workAreaRepository,
            IUserPointsRepository userPointsRepository,
            JwtHelper jwtHelper)
        {
            _userRepository       = userRepository;
            _workAreaRepository   = workAreaRepository;
            _userPointsRepository = userPointsRepository;
            _jwtHelper            = jwtHelper;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("account not found");

            return MapToDto(user);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest data, string? avatarUrl = null)
        {
            var user = await _userRepository.GetByIdTrackedAsync(userId)
                ?? throw new KeyNotFoundException("account not found");

            if (data.Fullname != null)
                user.FullName = data.Fullname;
            if (avatarUrl != null)
                user.AvatarUrl = avatarUrl;

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<UserProfileDto> UpdateLocationAsync(Guid userId, UpdateCitizenLocationRequest req)
        {
            var user = await _userRepository.GetByIdTrackedAsync(userId)
                ?? throw new KeyNotFoundException("account not found");

            if (user.Role != Common.Enums.UserRole.Citizen)
                throw new UnauthorizedAccessException("only citizens can update location");

            var workArea = await _workAreaRepository.GetByIdAsync(req.WardId)
                ?? throw new KeyNotFoundException("ward not found");

            if (workArea.Type != "Ward")
                throw new ArgumentException("ward_id must be a Ward-level work area");

            user.WorkAreaId = req.WardId;
            if (req.Address != null)
                user.Address = req.Address;

            // Sync denormalized cache in user_points for leaderboard scope=Area
            await _userPointsRepository.UpdateWorkAreaNameAsync(userId, workArea.Name);

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<string> ChangePasswordAsync(string email, ChangePasswordData data, CancellationToken ct = default)
        {
            var pwError = ValidationHelper.GetPasswordValidationError(data.NewPassword);
            if (pwError != null)
                throw new ArgumentException(pwError);

            var user = await _userRepository.GetByEmailTrackedAsync(email, ct)
                ?? throw new KeyNotFoundException("account not found");

            if (user.IsBanned)
                throw new UnauthorizedAccessException("forbidden");

            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !PasswordHelper.Verify(data.OldPassword, user.PasswordHash))
                throw new ArgumentException("old password is incorrect");

            if (PasswordHelper.Verify(data.NewPassword, user.PasswordHash))
                throw new ArgumentException("new password must be different from old password");

            user.PasswordHash = PasswordHelper.Hash(data.NewPassword);
            user.UpdatedAt    = DateTime.UtcNow;

            if (data.LogoutAllDevices)
                user.LoginTerm++;

            await _userRepository.SaveChangesAsync(ct);

            var (token, _) = _jwtHelper.GenerateAccessToken(user.Email, user.FullName, user.LoginTerm);
            return token;
        }

        private static UserProfileDto MapToDto(User u) => new()
        {
            Email      = u.Email,
            Fullname   = u.FullName,
            Role       = u.Role.ToString(),
            Address    = u.Address,
            AvatarUrl  = u.AvatarUrl,
            WorkAreaId = u.WorkAreaId,
            CreatedAt  = u.CreatedAt,
            UpdatedAt  = u.UpdatedAt
        };
    }
}
