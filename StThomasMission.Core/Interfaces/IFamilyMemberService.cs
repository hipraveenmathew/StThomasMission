using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyMemberService
    {
        Task AddFamilyMemberAsync(int familyId, string firstName, string lastName, FamilyMemberRole relation, DateTime dateOfBirth, string? contact, string? email, string? role);
        Task UpdateFamilyMemberAsync(int familyMemberId, string firstName, string lastName, FamilyMemberRole relation, DateTime dateOfBirth, string? contact, string? email, string? role);
        Task DeleteFamilyMemberAsync(int familyMemberId);
        Task<FamilyMember> GetFamilyMemberByIdAsync(int familyMemberId);
        Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId);
        Task<FamilyMember> GetFamilyMemberByUserIdAsync(string userId);
    }
}