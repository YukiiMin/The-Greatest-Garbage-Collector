using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class CollectorStaffRepository : ICollectorStaffRepository
    {
        private readonly AppDbContext _context;

        public CollectorStaffRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<CollectorStaff?> GetByUserIdAsync(Guid userId)
            => _context.CollectorStaffs
                .Include(s => s.User)
                .Include(s => s.Collector)
                .Include(s => s.CollectorHub)
                    .ThenInclude(h => h!.WorkArea)
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.UserId == userId);

        public Task<IEnumerable<CollectorStaff>> GetByCollectorIdAsync(Guid collectorId)
            => _context.CollectorStaffs
                .Include(s => s.User)
                .Include(s => s.Team)
                .Where(s => s.CollectorId == collectorId)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<CollectorStaff>)r.Result);

        public Task<IEnumerable<CollectorStaff>> GetByTeamIdAsync(Guid teamId)
            => _context.CollectorStaffs
                .Include(s => s.User)
                .Where(s => s.TeamId == teamId)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<CollectorStaff>)r.Result);

        public Task<IEnumerable<CollectorStaff>> GetByCollectorHubIdAsync(Guid collectorHubId)
            => _context.CollectorStaffs
                .Include(s => s.User)
                .Where(s => s.CollectorHubId == collectorHubId)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<CollectorStaff>)r.Result);

        public async Task<CollectorStaff> CreateAsync(CollectorStaff staff)
        {
            _context.CollectorStaffs.Add(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task<CollectorStaff> UpdateAsync(CollectorStaff staff)
        {
            _context.CollectorStaffs.Update(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task DeleteAsync(CollectorStaff staff)
        {
            _context.CollectorStaffs.Remove(staff);
            await _context.SaveChangesAsync();
        }

        public async Task SetTeamAsync(Guid userId, Guid? teamId)
        {
            var staff = await _context.CollectorStaffs.FindAsync(userId)
                ?? throw new KeyNotFoundException($"CollectorStaff not found for user {userId}");

            staff.TeamId     = teamId;
            staff.JoinTeamAt = teamId.HasValue ? DateTime.UtcNow : null;
            await _context.SaveChangesAsync();
        }
    }
}
