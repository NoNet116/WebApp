using BLL.ModelsDto;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        public Task<IEnumerable<UserDto>> GetAllUsersAsync();

        public Task<UserDto> GetUserByIdAsync(string id);

        Task<Result<UserDto>> CreateUserAsync(UserDto userDto);

        Task<Result<bool>> DeleteUserAsync(string id);

        Task<Result<string>> EditUserRoleAsync(string id, string role);
        Task<Result<UserDto>> EditUserAsync(string id, UserDto dto);
    }
}