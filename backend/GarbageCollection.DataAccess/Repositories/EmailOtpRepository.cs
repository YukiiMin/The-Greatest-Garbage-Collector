using GarbageCollection.Common.Models;
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
    public sealed class EmailOtpRepository : IEmailOtpRepository
    {
        private readonly AppDbContext _db;

        public EmailOtpRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(EmailOtp otp, CancellationToken ct = default)
        {
            await _db.EmailOtps.AddAsync(otp, ct);
        }

        public Task<EmailOtp?> GetActiveOtpAsync(string email, CancellationToken ct = default)
            => _db.EmailOtps
                  .AsNoTracking()
                  .Where(o => o.Email == email
                           && !o.IsUsed
                           && o.ExpiresAt > DateTime.UtcNow)
                  .OrderByDescending(o => o.CreatedAt)
                  .FirstOrDefaultAsync(ct);
        public Task<EmailOtp?> GetLatestByEmailAsync(string email, CancellationToken ct = default)
           => _db.EmailOtps
                 .AsNoTracking()
                 .Where(o => o.Email == email)
                 .OrderByDescending(o => o.CreatedAt)
                 .FirstOrDefaultAsync(ct);
        public async Task MarkUsedAsync(Guid otpId, CancellationToken ct = default)
        {
            await _db.EmailOtps
                     .Where(o => o.Id == otpId)
                     .ExecuteUpdateAsync(
                         s => s.SetProperty(o => o.IsUsed, true), ct);
        }
        public async Task UpdateAsync(
   Guid otpId,
   string newOtpCode,
   DateTime newExpiresAt,
   int newCount,
   DateTime updatedAt,
   CancellationToken ct = default)
        {
            await _db.EmailOtps
                     .Where(o => o.Id == otpId)
                     .ExecuteUpdateAsync(s => s
                         .SetProperty(o => o.OtpCode, newOtpCode)
                         .SetProperty(o => o.ExpiresAt, newExpiresAt)
                         .SetProperty(o => o.IsUsed, false)
                         .SetProperty(o => o.Count, newCount)
                         .SetProperty(o => o.UpdatedAt, updatedAt),
                     ct);
        }
        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
