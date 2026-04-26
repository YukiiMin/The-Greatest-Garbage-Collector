using GarbageCollection.Common.DTOs.User;

namespace GarbageCollection.Business.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest data, string? avatarUrl = null);
        Task<UserProfileDto> UpdateLocationAsync(Guid userId, UpdateCitizenLocationRequest req);

        /// <summary>
        /// Đổi mật khẩu. Trả về accessToken mới để controller set cookie.
        /// </summary>
        Task<string> ChangePasswordAsync(string email, ChangePasswordData data, CancellationToken ct = default);
    }
}
