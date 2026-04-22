using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByEmailTrackedAsync(string email, CancellationToken ct = default);
        Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);
        Task<User> CreateAsync(User user, CancellationToken ct = default);
<<<<<<< HEAD
        Task IncrementLoginTermAsync(Guid userId, CancellationToken ct = default);
=======
>>>>>>> 2b44a62e233f1c93c71d628b9c07ab83abfea1a0
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
