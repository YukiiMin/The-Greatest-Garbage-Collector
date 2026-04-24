using Microsoft.EntityFrameworkCore;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.DataAccess.Repositories
{
    public class CitizenReportRepository : ICitizenReportRepository
    {
        private readonly AppDbContext _context;

        public CitizenReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CitizenReport> CreateAsync(CitizenReport report)
        {
            _context.CitizenReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<CitizenReport?> GetByIdAsync(Guid id)
        {
            return await _context.CitizenReports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<CitizenReport>> GetByUserIdAsync(Guid userId, ReportStatus? status = null)
        {
            var query = _context.CitizenReports
                .Include(r => r.User)
                .Where(r => r.UserId == userId);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<CitizenReport> Items, int Total)> GetByUserIdPagedAsync(Guid userId, int page, int limit)
        {
            var query = _context.CitizenReports
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (items, total);
        }

        public async Task DeleteAsync(CitizenReport report)
        {
            _context.CitizenReports.Remove(report);
            await _context.SaveChangesAsync();
        }

        public async Task<CitizenReport> UpdateAsync(CitizenReport report)
        {
            report.UpdatedAt = DateTime.UtcNow;
            _context.CitizenReports.Update(report);
            await _context.SaveChangesAsync();
            return report;
        }
    }
}
