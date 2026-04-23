using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class EnterpriseRepository : IEnterpriseRepository
    {
        private readonly AppDbContext _context;

        public EnterpriseRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Enterprise?> GetByIdAsync(Guid id)
            => _context.Enterprises.FirstOrDefaultAsync(e => e.Id == id);

        public Task<IEnumerable<Enterprise>> GetAllAsync()
            => _context.Enterprises
                .OrderBy(e => e.Name)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<Enterprise>)r.Result);

        public async Task<Enterprise> CreateAsync(Enterprise enterprise)
        {
            _context.Enterprises.Add(enterprise);
            await _context.SaveChangesAsync();
            return enterprise;
        }

        public async Task<Enterprise> UpdateAsync(Enterprise enterprise)
        {
            enterprise.UpdatedAt = DateTime.UtcNow;
            _context.Enterprises.Update(enterprise);
            await _context.SaveChangesAsync();
            return enterprise;
        }

        public async Task DeleteAsync(Enterprise enterprise)
        {
            _context.Enterprises.Remove(enterprise);
            await _context.SaveChangesAsync();
        }
    }
}
