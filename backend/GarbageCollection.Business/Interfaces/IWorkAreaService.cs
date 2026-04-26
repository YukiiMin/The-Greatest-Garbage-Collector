using GarbageCollection.Common.DTOs.Admin;

namespace GarbageCollection.Business.Interfaces
{
    public interface IWorkAreaService
    {
        Task<List<WorkAreaDto>> GetAllAsync(string? type = null);
        Task<WorkAreaDto> GetByIdAsync(Guid id);
        Task<WorkAreaDto> CreateAsync(SaveWorkAreaRequest request);
        Task<WorkAreaDto> CreateDistrictAsync(CreateDistrictRequest request);
        Task<WorkAreaDto> CreateWardAsync(CreateWardRequest request);
        Task<WorkAreaDto> UpdateAsync(Guid id, SaveWorkAreaRequest request);
        Task DeleteAsync(Guid id);
    }
}
