using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyMemberService
    {
        Task AddFamilyMemberAsync(FamilyMember familyMember);
        Task UpdateFamilyMemberAsync(FamilyMember updatedMember);
        Task DeleteFamilyMemberAsync(int familyMemberId);
        Task<FamilyMember> GetFamilyMemberByIdAsync(int familyMemberId);
        Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId);
        Task<FamilyMember> GetFamilyMemberByUserIdAsync(string userId);
    }
}