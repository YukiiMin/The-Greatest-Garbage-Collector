using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly AppDbContext _context;

        public StaffRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Staff?> GetByUserIdAsync(Guid userId)
            => _context.Staffs
                .Include(s => s.User)
                .Include(s => s.Enterprise)
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.UserId == userId);

        public Task<IEnumerable<Staff>> GetByEnterpriseIdAsync(Guid enterpriseId)
            => _context.Staffs
                .Include(s => s.User)
                .Include(s => s.Team)
                .Where(s => s.EnterpriseId == enterpriseId)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<Staff>)r.Result);

        public Task<IEnumerable<Staff>> GetByTeamIdAsync(Guid teamId)
            => _context.Staffs
                .Include(s => s.User)
                .Where(s => s.TeamId == teamId)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<Staff>)r.Result);

        public async Task<Staff> CreateAsync(Staff staff)
        {
            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task<Staff> UpdateAsync(Staff staff)
        {
            _context.Staffs.Update(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task DeleteAsync(Staff staff)
        {
            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
        }
    }
}
