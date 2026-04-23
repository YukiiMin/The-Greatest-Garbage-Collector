using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class ReportDetailDto
    {
        public int Id { get; set; }
        public List<string> WasteCategories { get; set; } = new();
        public string WasteUnit { get; set; } = string.Empty;
        public List<string> CitizenImageUrls { get; set; } = new();
        public List<string> CollectorImageUrls { get; set; } = new();
        public decimal GpsLat { get; set; }
        public decimal GpsLng { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CollectedAt { get; set; }
        public string CitizenEmail { get; set; } = string.Empty;
    }
}
