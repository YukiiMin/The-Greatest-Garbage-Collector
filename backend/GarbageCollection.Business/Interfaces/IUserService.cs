using GarbageCollection.Common.DTOs.User;

namespace GarbageCollection.Business.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileData data);

        /// <summary>
        /// Đổi mật khẩu. Trả về accessToken mới để controller set cookie.
        /// </summary>
        Task<string> ChangePasswordAsync(string email, ChangePasswordData data, CancellationToken ct = default);
    }
}
