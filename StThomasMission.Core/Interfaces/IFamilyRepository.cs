using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyRepository : IRepository<Family>
    {
        Task<IEnumerable<Family>> GetByWardAsync(int wardId);
        Task<IEnumerable<Family>> GetByStatusAsync(FamilyStatus status);
        Task<Family?> GetByChurchRegistrationNumberAsync(string churchRegistrationNumber);
    }
}