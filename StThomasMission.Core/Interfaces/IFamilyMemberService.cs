using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing FamilyMember operations.
    /// </summary>
    public interface IFamilyMemberService
    {
        Task AddFamilyMemberAsync(int familyId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email, string? role);
        Task UpdateFamilyMemberAsync(int familyMemberId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email, string? role);

        Task<FamilyMember?> GetFamilyMemberByIdAsync(int familyMemberId);
        Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId);
    }
}