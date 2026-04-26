using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class WorkAreaRepository : IWorkAreaRepository
    {
        private readonly AppDbContext _context;

        public WorkAreaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkArea>> GetAllAsync(string? type = null)
        {
            var query = _context.WorkAreas
                .Include(x => x.Parent)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(x => x.Type == type);

            return await query.OrderBy(x => x.Type).ThenBy(x => x.Name).ToListAsync();
        }

        public Task<WorkArea?> GetByIdAsync(Guid id)
            => _context.WorkAreas
                .Include(x => x.Parent)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

        public Task<WorkArea?> GetByIdWithChildrenAsync(Guid id)
            => _context.WorkAreas
                .Include(x => x.Children)
                .Include(x => x.Parent)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<WorkArea> CreateAsync(WorkArea workArea)
        {
            _context.WorkAreas.Add(workArea);
            await _context.SaveChangesAsync();
            return workArea;
        }

        public async Task<WorkArea> UpdateAsync(WorkArea workArea)
        {
            _context.WorkAreas.Update(workArea);
            await _context.SaveChangesAsync();
            return workArea;
        }

        public async Task DeleteAsync(WorkArea workArea)
        {
            _context.WorkAreas.Remove(workArea);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasDependentsAsync(Guid id)
        {
            // Kiểm tra có entity nào FK vào work_area này không
            bool hasChildren = await _context.WorkAreas.AnyAsync(x => x.ParentId == id);
            if (hasChildren) return true;

            bool usedByEnterprise = await _context.Enterprises.AnyAsync(x => x.WorkAreaId == id);
            if (usedByEnterprise) return true;

            bool usedByCollector = await _context.Collectors.AnyAsync(x => x.WorkAreaId == id);
            if (usedByCollector) return true;

            bool usedByTeam = await _context.Teams.AnyAsync(x => x.WorkAreaId == id);
            if (usedByTeam) return true;

            bool usedByUser = await _context.Users.AnyAsync(x => x.WorkAreaId == id);
            return usedByUser;
        }
    }
}
