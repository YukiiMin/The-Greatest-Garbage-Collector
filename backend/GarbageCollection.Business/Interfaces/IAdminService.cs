using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Complaint;
using GarbageCollection.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    public interface IAdminService
    {
        /// <summary>
        /// Returns a paginated, filtered list of complaints for admin review.
        ///
        /// Steps:
        ///   1. Verify caller is "admin" role (via DB lookup by email)
        ///   2. Validate query parameters (status, page, limit)
        ///   3. Map status string → ComplaintStatus enum
        ///   4. Query repository (paginated list + count)
        ///   5. Build and return the response payload
        /// </summary>
        /// <param name="tokenEmail">Email extracted from the validated JWT by the controller.</param>
        /// <param name="request">Raw query parameters from the HTTP request.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<GetComplaintsResult> GetComplaintsAsync(
            string tokenEmail,
            GetComplaintsRequestDto request,
            CancellationToken ct = default);
       // Task<(Guid, ApiResponse<ComplaintDetailResponseDto>)> GetComplaintDetailAsync(
       //string email,
       //Guid complaintId,
       //CancellationToken ct = default);

       
    }
}
