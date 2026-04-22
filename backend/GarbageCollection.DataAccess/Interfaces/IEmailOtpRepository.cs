using GarbageCollection.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IEmailOtpRepository
    {
        /// <summary>Persists a new OTP record.</summary>
        Task CreateAsync(EmailOtp otp, CancellationToken ct = default);

        /// <summary>Returns the latest unused, non-expired OTP for an email.</summary>
        Task<EmailOtp?> GetActiveOtpAsync(string email, CancellationToken ct = default);

        Task<EmailOtp?> GetLatestByEmailAsync(string email, CancellationToken ct = default);

        /// <summary>Marks an OTP as used.</summary>
        Task MarkUsedAsync(Guid otpId, CancellationToken ct = default);

        Task UpdateAsync(
     Guid otpId,
     string newOtpCode,
     DateTime newExpiresAt,
     int newCount,
     DateTime updatedAt,
     CancellationToken ct = default);


        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
