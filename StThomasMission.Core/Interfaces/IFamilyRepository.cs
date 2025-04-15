using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Repository for Family-specific operations.
    /// </summary>
    public interface IFamilyRepository : IRepository<Family>
    {
        Task<IEnumerable<Family>> GetByWardAsync(string ward);
        Task<IEnumerable<Family>> GetByStatusAsync(string status);
    }
}
