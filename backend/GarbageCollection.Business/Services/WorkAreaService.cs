using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs.Admin;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class WorkAreaService : IWorkAreaService
    {
        private readonly IWorkAreaRepository _repo;

        public WorkAreaService(IWorkAreaRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<WorkAreaDto>> GetAllAsync(string? type = null)
        {
            var list = await _repo.GetAllAsync(type);
            return list.Select(MapToDto).ToList();
        }

        public async Task<WorkAreaDto> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdWithChildrenAsync(id)
                ?? throw new KeyNotFoundException("Work area not found");
            return MapToDto(entity);
        }

        public async Task<WorkAreaDto> CreateAsync(SaveWorkAreaRequest request)
        {
            var data = request.Data;
            var type = data.Type.Trim();

            if (type != "District" && type != "Ward")
                throw new ArgumentException("Type must be 'District' or 'Ward'");

            if (type == "Ward")
            {
                if (data.ParentId == null)
                    throw new ArgumentException("Ward requires a parent_id (District)");

                var parent = await _repo.GetByIdAsync(data.ParentId.Value)
                    ?? throw new KeyNotFoundException("Parent district not found");

                if (parent.Type != "District")
                    throw new ArgumentException("Parent must be a District");
            }

            var entity = new WorkArea
            {
                Name = data.Name.Trim(),
                Type = type,
                ParentId = type == "Ward" ? data.ParentId : null,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(entity);

            // Reload with parent info
            var loaded = await _repo.GetByIdWithChildrenAsync(created.Id);
            return MapToDto(loaded!);
        }

        public async Task<WorkAreaDto> CreateDistrictAsync(CreateDistrictRequest request)
        {
            var entity = new WorkArea
            {
                Name      = request.Data.Name.Trim(),
                Type      = "District",
                ParentId  = null,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(entity);
            var loaded  = await _repo.GetByIdWithChildrenAsync(created.Id);
            return MapToDto(loaded!);
        }

        public async Task<WorkAreaDto> CreateWardAsync(CreateWardRequest request)
        {
            if (request.Data.ParentId == null)
                throw new ArgumentException("parent_id is required for Ward");

            var parent = await _repo.GetByIdAsync(request.Data.ParentId.Value)
                ?? throw new KeyNotFoundException("Parent district not found");

            if (parent.Type != "District")
                throw new ArgumentException("Parent must be a District");

            var entity = new WorkArea
            {
                Name      = request.Data.Name.Trim(),
                Type      = "Ward",
                ParentId  = request.Data.ParentId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(entity);
            var loaded  = await _repo.GetByIdWithChildrenAsync(created.Id);
            return MapToDto(loaded!);
        }

        public async Task<WorkAreaDto> UpdateAsync(Guid id, SaveWorkAreaRequest request)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Work area not found");

            var data = request.Data;
            var type = data.Type.Trim();

            if (type != "District" && type != "Ward")
                throw new ArgumentException("Type must be 'District' or 'Ward'");

            if (type == "Ward")
            {
                if (data.ParentId == null)
                    throw new ArgumentException("Ward requires a parent_id (District)");

                if (data.ParentId == id)
                    throw new ArgumentException("Work area cannot be its own parent");

                var parent = await _repo.GetByIdAsync(data.ParentId.Value)
                    ?? throw new KeyNotFoundException("Parent district not found");

                if (parent.Type != "District")
                    throw new ArgumentException("Parent must be a District");

                entity.ParentId = data.ParentId;
            }
            else
            {
                entity.ParentId = null;
            }

            entity.Name = data.Name.Trim();
            entity.Type = type;

            await _repo.UpdateAsync(entity);

            var loaded = await _repo.GetByIdWithChildrenAsync(entity.Id);
            return MapToDto(loaded!);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Work area not found");

            if (await _repo.HasDependentsAsync(id))
                throw new InvalidOperationException("Cannot delete: work area is referenced by other entities");

            await _repo.DeleteAsync(entity);
        }

        private static WorkAreaDto MapToDto(WorkArea w) => new()
        {
            Id = w.Id,
            Name = w.Name,
            Type = w.Type,
            ParentId = w.ParentId,
            ParentName = w.Parent?.Name,
            Children = w.Children.Select(MapToDto).ToList(),
            CreatedAt = w.CreatedAt
        };
    }
}
