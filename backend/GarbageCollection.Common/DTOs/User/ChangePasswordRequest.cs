using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("old_password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "new_password is required")]
        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "logout_all_devices is required")]
        [JsonPropertyName("logout_all_devices")]
        public bool LogoutAllDevices { get; set; }
    }
}
