using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class AuditLogDto
    {
        public string Action { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Note { get; set; }
    }
}
