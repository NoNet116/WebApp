using BLL.ModelsDto;

namespace BLL.Interfaces
{
    public interface IRoleService
    {
        Task<Result<RoleDto>> Create(string name);

        Task<Result<RoleDto>> UpdateAsync(RoleDto roleDto);

        Task<Result<bool>> Delete(string id);

        Task<IEnumerable<RoleDto>> GetAllAsync();

        Task<Result<RoleDto>> GetByNameAsync(string name);

        Task<Result<RoleDto>> GetByIdAsync(string id);

        Task<Result<IEnumerable<RoleDto>>> GetByNamesAsync(string name);
    }
}