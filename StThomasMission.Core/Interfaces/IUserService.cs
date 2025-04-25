using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing user operations.
    /// </summary>
    public interface IUserService
    {
        Task CreateUserAsync(string email, string fullName, int wardId, string password, string role);
        Task UpdateUserAsync(string userId, string fullName, int wardId, string? designation);
        Task AssignRoleAsync(string userId, string role);
        Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
    }
}