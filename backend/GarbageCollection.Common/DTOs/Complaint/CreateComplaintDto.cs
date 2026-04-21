using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class CreateComplaintDto
    {
        [Required(ErrorMessage = "reason is required.")]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public IList<IFormFile> Images { get; set; } = [];
    }
}
