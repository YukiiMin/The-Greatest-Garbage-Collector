using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class SendComplaintMessageRequest
    {
        [Required]
        public SendComplaintMessageData Data { get; set; } = new();
    }

    public class SendComplaintMessageData
    {
        [Required(ErrorMessage = "message is required")]
        [MaxLength(1000, ErrorMessage = "message must not exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;
    }
}
