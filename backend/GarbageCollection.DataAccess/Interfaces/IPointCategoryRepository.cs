using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IPointCategoryRepository
    {
        Task<PointCategory?> GetByIdAsync(int id);
        Task<IEnumerable<PointCategory>> GetByEnterpriseIdAsync(int enterpriseId);
        Task<PointCategory> CreateAsync(PointCategory category);
        Task<PointCategory> UpdateAsync(PointCategory category);
        Task DeleteAsync(PointCategory category);
    }
}
