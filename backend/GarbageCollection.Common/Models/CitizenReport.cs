using GarbageCollection.Common.Enums;

namespace GarbageCollection.Common.Models
{
    public class CitizenReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<string> CitizenImageUrls { get; set; } = [];

        public List<WasteType> Types { get; set; } = [];

        public decimal? Capacity { get; set; } // kg

        public string? Description { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public Guid UserId { get; set; }

        public Guid CitizenId { get; set; }
        public User Citizen { get; set; } = null!;


        public Guid? PointCategoryId { get; set; }


        public int? Point { get; set; }

        public Guid? TeamId { get; set; }

        public string? ReportNote { get; set; }

        public Guid? AssignBy { get; set; }

        public DateTime? AssignAt { get; set; }

        public DateTime? Deadline { get; set; }

        public DateTime? StartCollectingAt { get; set; }

        public DateTime? CollectedAt { get; set; }

        public DateTime ReportAt { get; set; } = DateTime.UtcNow;

        public List<string> CollectorImageUrls { get; set; } = [];

        public DateTime? CompleteAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;

    }
}
