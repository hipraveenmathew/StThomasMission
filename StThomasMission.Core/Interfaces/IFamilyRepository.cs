using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyRepository : IRepository<Family>
    {
        Task<IEnumerable<Family>> GetByWardAsync(string ward);
        Task<IEnumerable<Family>> GetByStatusAsync(string status);
    }
}