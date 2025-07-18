using StThomasMission.Core.DTOs;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(string userId);

        Task<IPaginatedList<UserDto>> SearchUsersPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? role = null);

        Task<UserDto> CreateUserAsync(CreateUserRequest request, string performedByUserId);

        Task UpdateUserAsync(string userId, UpdateUserRequest request, string performedByUserId);
        Task<List<string>> GetAllRolesAsync();
        Task ReactivateUserAsync(string userId, string performedByUserId);


        Task UpdateUserRoleAsync(string userId, string newRole, string performedByUserId);


        Task DeactivateUserAsync(string userId, string performedByUserId);
    }
}