using StThomasMission.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupService
    {
        Task<GroupDetailDto> GetGroupByIdAsync(int groupId);

        Task<IEnumerable<GroupDetailDto>> GetAllGroupsAsync();

        Task<GroupDetailDto> CreateGroupAsync(CreateGroupRequest request, string userId);

        Task UpdateGroupAsync(int groupId, UpdateGroupRequest request, string userId);

        Task DeleteGroupAsync(int groupId, string userId);
    }
}