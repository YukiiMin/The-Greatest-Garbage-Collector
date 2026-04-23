using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public sealed class GetComplaintsRequestDto
    {
        /// <summary>Raw status string from the query string (e.g. "PENDING", "RESOLVED").</summary>
        public string Status { get; set; } = "PENDING";

        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }
}
