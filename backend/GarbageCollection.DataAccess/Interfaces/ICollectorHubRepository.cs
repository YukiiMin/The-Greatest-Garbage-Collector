using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICollectorHubRepository
    {
        Task<CollectorHub?> GetByIdAsync(Guid id);
        Task<IEnumerable<CollectorHub>> GetAllAsync();
        Task<CollectorHub> CreateAsync(CollectorHub hub);
        Task<CollectorHub> UpdateAsync(CollectorHub hub);
        Task DeleteAsync(CollectorHub hub);
    }
}
