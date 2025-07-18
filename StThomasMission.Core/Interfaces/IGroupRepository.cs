using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<GroupDetailDto?> GetByNameAsync(string name);

        Task<IEnumerable<GroupDetailDto>> GetAllWithDetailsAsync();
    }
}