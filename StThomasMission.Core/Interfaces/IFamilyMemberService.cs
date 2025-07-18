using StThomasMission.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyMemberService
    {
        Task<FamilyMemberDto> GetFamilyMemberByIdAsync(int familyMemberId);

        Task<IEnumerable<FamilyMemberDto>> GetMembersByFamilyIdAsync(int familyId);

        Task<FamilyMemberDto> AddMemberToFamilyAsync(CreateFamilyMemberRequest request, string userId);

        Task UpdateFamilyMemberAsync(int familyMemberId, UpdateFamilyMemberRequest request, string userId);

        Task DeleteFamilyMemberAsync(int familyMemberId, string userId);
    }
}