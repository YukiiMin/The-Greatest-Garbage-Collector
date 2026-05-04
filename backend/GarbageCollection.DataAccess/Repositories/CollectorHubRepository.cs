using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class CollectorHubRepository : ICollectorHubRepository
    {
        private readonly AppDbContext _context;

        public CollectorHubRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<CollectorHub?> GetByIdAsync(Guid id)
            => _context.CollectorHubs
                .Include(h => h.WorkArea)
                .FirstOrDefaultAsync(h => h.Id == id);

        public Task<IEnumerable<CollectorHub>> GetAllAsync()
            => _context.CollectorHubs
                .Include(h => h.WorkArea)
                .OrderBy(h => h.Name)
                .ToListAsync()
                .ContinueWith(t => (IEnumerable<CollectorHub>)t.Result);

        public async Task<CollectorHub> CreateAsync(CollectorHub hub)
        {
            _context.CollectorHubs.Add(hub);
            await _context.SaveChangesAsync();
            return hub;
        }

        public async Task<CollectorHub> UpdateAsync(CollectorHub hub)
        {
            hub.UpdatedAt = DateTime.UtcNow;
            _context.CollectorHubs.Update(hub);
            await _context.SaveChangesAsync();
            return hub;
        }

        public async Task DeleteAsync(CollectorHub hub)
        {
            _context.CollectorHubs.Remove(hub);
            await _context.SaveChangesAsync();
        }
    }
}
