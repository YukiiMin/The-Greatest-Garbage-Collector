using GarbageCollection.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarbageCollection.Common.Models
{
    public class Complaint
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("citizen_id")]
        public Guid CitizenId { get; set; }
        [Column("report_id")]
        public Guid ReportId { get; set; }
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
