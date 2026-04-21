using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.User
{
    public class ChangePasswordRequest
    {
        [Required]
        public ChangePasswordData Data { get; set; } = new();
    }

    public class ChangePasswordData
    {
        [Required(ErrorMessage = "old_password is required")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "new_password is required")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "logout_all_devices is required")]
        public bool LogoutAllDevices { get; set; }
    }
}
