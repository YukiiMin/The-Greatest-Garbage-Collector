using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICollectorRepository
    {
        Task<Collector?> GetByIdAsync(int id);
        Task<IEnumerable<Collector>> GetByEnterpriseIdAsync(int enterpriseId);
        Task<Collector> CreateAsync(Collector collector);
        Task<Collector> UpdateAsync(Collector collector);
        Task DeleteAsync(Collector collector);
    }
}
