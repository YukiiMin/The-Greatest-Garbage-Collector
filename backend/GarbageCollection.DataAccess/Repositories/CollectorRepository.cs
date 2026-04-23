using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class CollectorRepository : ICollectorRepository
    {
        private readonly AppDbContext _context;

        public CollectorRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Collector?> GetByIdAsync(Guid id)
            => _context.Collectors
                .Include(c => c.Enterprise)
                .FirstOrDefaultAsync(c => c.Id == id);

        public Task<IEnumerable<Collector>> GetByEnterpriseIdAsync(Guid enterpriseId)
            => _context.Collectors
                .Where(c => c.EnterpriseId == enterpriseId)
                .OrderBy(c => c.Name)
                .ToListAsync()
                .ContinueWith(t => (IEnumerable<Collector>)t.Result);

        public async Task<Collector> CreateAsync(Collector collector)
        {
            _context.Collectors.Add(collector);
            await _context.SaveChangesAsync();
            return collector;
        }

        public async Task<Collector> UpdateAsync(Collector collector)
        {
            collector.UpdatedAt = DateTime.UtcNow;
            _context.Collectors.Update(collector);
            await _context.SaveChangesAsync();
            return collector;
        }

        public async Task DeleteAsync(Collector collector)
        {
            _context.Collectors.Remove(collector);
            await _context.SaveChangesAsync();
        }
    }
}
