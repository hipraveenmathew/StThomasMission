using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupService
    {
        Task AddGroupAsync(string name, string? description);
        Task UpdateGroupAsync(int groupId, string name, string? description);
        Task DeleteGroupAsync(int groupId);
        Task<Group?> GetGroupByIdAsync(int groupId);
        Task<IEnumerable<Group>> GetAllGroupsAsync();
    }
}