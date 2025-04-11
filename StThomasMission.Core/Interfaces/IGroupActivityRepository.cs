using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupActivityRepository
    {
        Task<GroupActivity> AddAsync(GroupActivity groupActivity);
        Task<IEnumerable<GroupActivity>> GetByGroupNameAsync(string groupName);
    }
}