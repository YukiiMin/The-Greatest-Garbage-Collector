using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class TeamDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool InWork { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalCapacity { get; set; }
        public Guid CollectorHubId { get; set; }
        public string CollectorHubName { get; set; } = string.Empty;
        public string? DispatchTime { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SaveTeamData
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid CollectorHubId { get; set; }

        public decimal TotalCapacity { get; set; }

        public bool IsActive { get; set; } = true;

        public string? DispatchTime { get; set; }
    }

    public class SaveTeamRequest
    {
        [Required]
        public SaveTeamData Data { get; set; } = null!;
    }
}
