using GarbageCollection.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class PointCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public PointMechanic Mechanic { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SavePointCategoryData
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public PointMechanic Mechanic { get; set; } = new();

        public bool IsActive { get; set; } = true;
    }

    public class SavePointCategoryRequest
    {
        [Required]
        public SavePointCategoryData Data { get; set; } = null!;
    }
}
