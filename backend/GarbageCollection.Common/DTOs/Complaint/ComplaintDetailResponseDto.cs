using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class ComplaintDetailResponseDto
    {
        public ComplaintDetailDto Complaint { get; set; } = new();
        public ReportDetailDto Report { get; set; } = new();
        public List<AuditLogDto> AuditTimeline { get; set; } = new();
    }

}
