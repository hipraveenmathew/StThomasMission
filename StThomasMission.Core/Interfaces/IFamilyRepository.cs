using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Repository for Family-specific operations.
    /// </summary>
    public interface IFamilyRepository : IRepository<Family>
    {
        Task<IEnumerable<Family>> GetByWardAsync(int wardId);
        Task<IEnumerable<Family>> GetByStatusAsync(FamilyStatus status);
    }
}