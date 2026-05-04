using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IPointTransactionRepository
    {
        Task<IEnumerable<PointTransaction>> GetByUserIdAsync(Guid userId);
        Task<PointTransaction> CreateAsync(PointTransaction transaction);
    }
}
