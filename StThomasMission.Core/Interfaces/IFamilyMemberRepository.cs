using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyMemberRepository : IRepository<FamilyMember>
    {
        Task<IEnumerable<FamilyMemberDto>> GetByFamilyIdAsync(int familyId);

        Task<FamilyMemberDto?> GetByUserIdAsync(string userId);
    }
}