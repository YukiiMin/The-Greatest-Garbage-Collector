using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IEnterpriseStaffRepository
    {
        Task<EnterpriseStaff?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<EnterpriseStaff>> GetByEnterpriseIdAsync(Guid enterpriseId);
        Task<EnterpriseStaff> CreateAsync(EnterpriseStaff staff);
        Task<EnterpriseStaff> UpdateAsync(EnterpriseStaff staff);
        Task DeleteAsync(EnterpriseStaff staff);
    }
}
