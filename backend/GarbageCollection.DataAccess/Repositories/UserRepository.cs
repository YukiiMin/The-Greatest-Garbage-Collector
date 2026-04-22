using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

        public Task<User?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<User?> GetByEmailTrackedAsync(string email, CancellationToken ct = default)
            => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
            => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);


        public async Task IncrementLoginTermAsync(Guid userId, CancellationToken ct = default)
        {
            await _db.Users
                     .Where(u => u.Id == userId)
                     .ExecuteUpdateAsync(
                         s => s.SetProperty(u => u.LoginTerm, u => u.LoginTerm + 1),
                     ct);
        }

        public async Task<User> CreateAsync(User user, CancellationToken ct = default)
        {
            await _db.Users.AddAsync(user, ct);
            return user;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
