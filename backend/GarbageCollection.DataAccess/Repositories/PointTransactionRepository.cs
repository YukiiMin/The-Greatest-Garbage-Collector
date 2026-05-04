using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class PointTransactionRepository : IPointTransactionRepository
    {
        private readonly AppDbContext _context;

        public PointTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<PointTransaction>> GetByUserIdAsync(Guid userId)
            => _context.PointTransactions
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync()
                .ContinueWith(t => (IEnumerable<PointTransaction>)t.Result);

        public async Task<PointTransaction> CreateAsync(PointTransaction transaction)
        {
            _context.PointTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }
    }
}
