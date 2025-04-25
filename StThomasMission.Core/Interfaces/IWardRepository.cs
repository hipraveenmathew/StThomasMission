using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Repository for Ward-specific operations.
    /// </summary>
    public interface IWardRepository : IRepository<Ward>
    {
        Task<Ward?> GetByNameAsync(string name);
        Task<IEnumerable<Ward>> GetByStatusAsync(bool isActive);
    }
}