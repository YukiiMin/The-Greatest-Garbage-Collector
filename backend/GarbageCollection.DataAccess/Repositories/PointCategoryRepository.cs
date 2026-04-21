using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class PointCategoryRepository : IPointCategoryRepository
    {
        private readonly AppDbContext _context;

        public PointCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<PointCategory?> GetByIdAsync(int id)
            => _context.PointCategories
                .Include(p => p.Enterprise)
                .FirstOrDefaultAsync(p => p.Id == id);

        public Task<IEnumerable<PointCategory>> GetByEnterpriseIdAsync(int enterpriseId)
            => _context.PointCategories
                .Where(p => p.EnterpriseId == enterpriseId)
                .OrderBy(p => p.Name)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<PointCategory>)r.Result);

        public async Task<PointCategory> CreateAsync(PointCategory category)
        {
            _context.PointCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<PointCategory> UpdateAsync(PointCategory category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.PointCategories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteAsync(PointCategory category)
        {
            _context.PointCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
