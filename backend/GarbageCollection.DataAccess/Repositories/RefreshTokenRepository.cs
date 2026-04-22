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
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;

        public RefreshTokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(RefreshToken token, CancellationToken ct = default)
        {
            await _db.RefreshTokens.AddAsync(token, ct);
        }

        public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
        {
            await _db.RefreshTokens
                     .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                     .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true), ct);
        }
<<<<<<< HEAD
        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
            => _db.RefreshTokens
                  .AsNoTracking()
                  .Include(rt => rt.User)
                  .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);
        public async Task RevokeByIdAsync(Guid tokenId, CancellationToken ct = default)
        {
            await _db.RefreshTokens
                     .Where(rt => rt.Id == tokenId)
                     .ExecuteUpdateAsync(
                         s => s.SetProperty(rt => rt.IsRevoked, true), ct);
        }
=======
>>>>>>> 2b44a62e233f1c93c71d628b9c07ab83abfea1a0

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
