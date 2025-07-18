using StThomasMission.Core.DTOs;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupActivityService
    {
        Task<GroupActivityDto> CreateGroupActivityAsync(CreateGroupActivityRequest request, string userId);

        Task UpdateGroupActivityAsync(int groupActivityId, UpdateGroupActivityRequest request, string userId);

        Task DeleteGroupActivityAsync(int groupActivityId, string userId);

        Task AssignStudentsToActivityAsync(int groupActivityId, AssignStudentsToActivityRequest request, string userId);
    }
}