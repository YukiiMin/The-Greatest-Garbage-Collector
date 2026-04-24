using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICollectorRepository
    {
        Task<Collector?> GetByIdAsync(Guid id);
        Task<IEnumerable<Collector>> GetByEnterpriseIdAsync(Guid enterpriseId);
        Task<Collector> CreateAsync(Collector collector);
        Task<Collector> UpdateAsync(Collector collector);
        Task DeleteAsync(Collector collector);
    }
}
