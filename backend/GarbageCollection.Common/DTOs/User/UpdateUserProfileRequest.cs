using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.User
{
    public class UpdateUserProfileRequest
    {
        [Required]
        public UpdateUserProfileData Data { get; set; } = new();
    }

    public class UpdateUserProfileData
    {
        [Required(ErrorMessage = "fullname is required")]
        [MaxLength(256, ErrorMessage = "fullname must not exceed 256 characters")]
        public string Fullname { get; set; } = string.Empty;

        [MaxLength(512, ErrorMessage = "address must not exceed 512 characters")]
        public string? Address { get; set; }

        [Url(ErrorMessage = "avatar_url must be a valid URL")]
        [MaxLength(1024)]
        public string? AvatarUrl { get; set; }
    }
}
