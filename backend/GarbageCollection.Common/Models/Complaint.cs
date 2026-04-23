using GarbageCollection.Common.Enums;

namespace GarbageCollection.Common.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public Guid CitizenId { get; set; }
        public int ReportId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
        public string? AdminResponse { get; set; }
        public List<ComplaintMessage> Messages { get; set; } = [];
        public DateTime RequestAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResponseAt { get; set; }

        // Navigation
        public CitizenReport Report { get; set; } = null!;
        public User Citizen { get; set; } = null!;
    }
}
