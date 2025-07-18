using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IWardRepository : IRepository<Ward>
    {
        Task<WardDetailDto?> GetByNameAsync(string name);

        Task<IEnumerable<WardDetailDto>> GetAllWithDetailsAsync();
    }
}