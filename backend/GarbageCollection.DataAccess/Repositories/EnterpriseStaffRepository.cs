using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class EnterpriseStaffRepository : IEnterpriseStaffRepository
    {
        private readonly AppDbContext _context;

        public EnterpriseStaffRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<EnterpriseStaff?> GetByUserIdAsync(Guid userId)
            => _context.EnterpriseStaffs
                .Include(s => s.User)
                .Include(s => s.Enterprise)
                .FirstOrDefaultAsync(s => s.UserId == userId);

        public Task<IEnumerable<EnterpriseStaff>> GetByEnterpriseIdAsync(Guid enterpriseId)
            => _context.EnterpriseStaffs
                .Include(s => s.User)
                .Where(s => s.EnterpriseId == enterpriseId)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<EnterpriseStaff>)r.Result);

        public async Task<EnterpriseStaff> CreateAsync(EnterpriseStaff staff)
        {
            _context.EnterpriseStaffs.Add(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task<EnterpriseStaff> UpdateAsync(EnterpriseStaff staff)
        {
            _context.EnterpriseStaffs.Update(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task DeleteAsync(EnterpriseStaff staff)
        {
            _context.EnterpriseStaffs.Remove(staff);
            await _context.SaveChangesAsync();
        }
    }
}
