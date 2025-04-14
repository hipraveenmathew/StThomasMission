using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupActivityRepository
    {
        Task<GroupActivity> GetByIdAsync(int id);
        Task<IEnumerable<GroupActivity>> GetAllAsync();
        Task<IEnumerable<GroupActivity>> GetByGroupAsync(string group); // Add this
        Task AddAsync(GroupActivity groupActivity);
        Task UpdateAsync(GroupActivity groupActivity);
        Task DeleteAsync(int id);
    }
}