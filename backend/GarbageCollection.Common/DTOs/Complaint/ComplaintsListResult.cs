using GarbageCollection.Common.DTOs;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class ComplaintsListResult
    {
        public List<ComplaintResponseDto> Complaints { get; set; } = [];
        public PaginationMeta Pagination { get; set; } = new();
    }
}
