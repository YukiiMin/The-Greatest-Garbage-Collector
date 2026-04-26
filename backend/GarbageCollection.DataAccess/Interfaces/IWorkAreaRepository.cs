using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IWorkAreaRepository
    {
        Task<List<WorkArea>> GetAllAsync(string? type = null);
        Task<WorkArea?> GetByIdAsync(Guid id);
        Task<WorkArea?> GetByIdWithChildrenAsync(Guid id);
        Task<WorkArea> CreateAsync(WorkArea workArea);
        Task<WorkArea> UpdateAsync(WorkArea workArea);
        Task DeleteAsync(WorkArea workArea);
        Task<bool> HasDependentsAsync(Guid id);
    }
}
