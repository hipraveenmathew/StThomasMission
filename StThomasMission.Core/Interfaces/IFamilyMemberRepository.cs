using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyMemberRepository : IRepository<FamilyMember>
    {
        Task<IEnumerable<FamilyMember>> GetByFamilyIdAsync(int familyId);
        Task<FamilyMember> GetByUserIdAsync(string userId);
    }
}