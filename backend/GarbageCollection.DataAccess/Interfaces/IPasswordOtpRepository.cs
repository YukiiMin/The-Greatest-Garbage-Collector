using GarbageCollection.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IPasswordOtpRepository
    {
        /// <summary>
        /// Returns the PasswordOtp record for the given email, or null if none exists.
        /// The service layer owns all validity decisions (expired, used, etc.).
        /// </summary>
        Task<PasswordOtp?> GetByEmailAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Inserts a new PasswordOtp record (first-time reset request for this email).
        /// Call SaveChangesAsync to commit.
        /// </summary>
        Task CreateAsync(PasswordOtp otp, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing PasswordOtp record in-place (subsequent reset requests):
        /// replaces OtpCode hash, resets ExpiresAt and IsUsed, increments Count,
        /// and sets LastUpdatedAt = now.
        /// Commits immediately via ExecuteUpdateAsync — no SaveChangesAsync required.
        /// </summary>
        Task UpdateAsync(
            string email,
            string newOtpCodeHash,
            DateTime newExpiresAt,
            int newCount,
            DateTime lastUpdatedAt,
            CancellationToken ct = default);

        /// <summary>
        /// Marks the PasswordOtp record for this email as used.
        /// Commits immediately via ExecuteUpdateAsync.
        /// </summary>
        Task MarkUsedAsync(string email, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
