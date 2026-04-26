using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class AssignReportData
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public DateTime Deadline { get; set; }
    }

    public class AssignReportRequest
    {
        [Required]
        public AssignReportData Data { get; set; } = null!;
    }
}
