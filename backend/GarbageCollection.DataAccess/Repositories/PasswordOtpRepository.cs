using GarbageCollection.Common.DTOs;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.DataAccess.Repositories
{
    public sealed class PasswordOtpRepository : IPasswordOtpRepository
    {
        private readonly AppDbContext _db;

        public PasswordOtpRepository(AppDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public Task<PasswordOtp?> GetByEmailAsync(string email, CancellationToken ct = default)
            => _db.PasswordOtps
                  .AsNoTracking()
                  .FirstOrDefaultAsync(o => o.Email == email, ct);

        /// <inheritdoc/>
        public async Task CreateAsync(PasswordOtp otp, CancellationToken ct = default)
        {
            await _db.PasswordOtps.AddAsync(otp, ct);
        }

        /// <inheritdoc/>
        /// Uses ExecuteUpdateAsync for a direct SQL UPDATE — no entity load,
        /// no change-tracker involvement, commits atomically in one round-trip.
        public async Task UpdateAsync(
            string email,
            string newOtpCodeHash,
            DateTime newExpiresAt,
            int newCount,
            DateTime lastUpdatedAt,
            CancellationToken ct = default)
        {
            await _db.PasswordOtps
                     .Where(o => o.Email == email)
                     .ExecuteUpdateAsync(s => s
                         .SetProperty(o => o.OtpCode, newOtpCodeHash)
                         .SetProperty(o => o.ExpiresAt, newExpiresAt)
                         .SetProperty(o => o.IsUsed, false)
                         .SetProperty(o => o.Count, newCount)
                         .SetProperty(o => o.LastUpdatedAt, lastUpdatedAt),
                     ct);
        }

        /// <inheritdoc/>
        public async Task MarkUsedAsync(string email, CancellationToken ct = default)
        {
            await _db.PasswordOtps
                     .Where(o => o.Email == email)
                     .ExecuteUpdateAsync(
                         s => s.SetProperty(o => o.IsUsed, true), ct);
        }

        /// <inheritdoc/>
        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
