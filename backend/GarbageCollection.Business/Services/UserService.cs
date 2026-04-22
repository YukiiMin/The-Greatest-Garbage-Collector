using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs.User;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;

        public UserService(IUserRepository userRepository, JwtHelper jwtHelper)
        {
            _userRepository = userRepository;
            _jwtHelper      = jwtHelper;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("account not found");

            return MapToDto(user);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileData data, string? avatarUrl = null)
        {
            var user = await _userRepository.GetByIdTrackedAsync(userId)
                ?? throw new KeyNotFoundException("account not found");

            user.FullName  = data.Fullname;
            user.Address   = data.Address;
            if (avatarUrl != null)
                user.AvatarUrl = avatarUrl;
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
            Email     = u.Email,
            Fullname  = u.FullName,
            Address   = u.Address,
            AvatarUrl = u.AvatarUrl,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        };
    }
}
