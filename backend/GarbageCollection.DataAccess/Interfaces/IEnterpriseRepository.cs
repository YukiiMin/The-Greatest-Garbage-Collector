using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IEnterpriseRepository
    {
        Task<Enterprise?> GetByIdAsync(int id);
        Task<IEnumerable<Enterprise>> GetAllAsync();
        Task<Enterprise> CreateAsync(Enterprise enterprise);
        Task<Enterprise> UpdateAsync(Enterprise enterprise);
        Task DeleteAsync(Enterprise enterprise);
    }
}
