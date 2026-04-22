using Microsoft.AspNetCore.Http;
using GarbageCollection.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.CitizenReport
{
    public class CreateCitizenReportDto
    {
        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 ảnh.")]
        public IList<IFormFile> Images { get; set; } = [];

        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 loại rác.")]
        public IList<WasteType> Types { get; set; } = [];

        [Range(0.01, 10000, ErrorMessage = "Khối lượng phải lớn hơn 0 và không vượt quá 10000 kg.")]
        public decimal? Capacity { get; set; }

        [MaxLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}
