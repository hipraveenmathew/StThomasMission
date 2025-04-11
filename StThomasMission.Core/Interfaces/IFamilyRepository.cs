using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyRepository
    {
        Task<Family> AddAsync(Family family);
        Task<Family?> GetByIdAsync(int id);
        Task UpdateAsync(Family family);
        Task DeleteAsync(int id);
        Task<IEnumerable<Family>> GetAllAsync();
    }
}