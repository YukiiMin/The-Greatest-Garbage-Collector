using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Staff>> GetByEnterpriseIdAsync(Guid enterpriseId);
        Task<IEnumerable<Staff>> GetByTeamIdAsync(Guid teamId);
        Task<Staff> CreateAsync(Staff staff);
        Task<Staff> UpdateAsync(Staff staff);
        Task DeleteAsync(Staff staff);
    }
}
