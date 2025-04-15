using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing Family and FamilyMember operations.
    /// </summary>
    public interface IFamilyService
    {
        Task<Family> RegisterFamilyAsync(string familyName, string ward, bool isRegistered, string? churchRegistrationNumber, string? temporaryId);
        Task UpdateFamilyAsync(int familyId, string familyName, string ward, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, string status, string? migratedTo);
        Task ConvertTemporaryIdToChurchIdAsync(int familyId, string churchRegistrationNumber);

        Task<Family> GetFamilyByIdAsync(int familyId);
        Task<IEnumerable<Family>> GetFamiliesByWardAsync(string ward);
        Task<IEnumerable<Family>> GetFamiliesByStatusAsync(string status);

        Task AddFamilyMemberAsync(int familyId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email, string? role);
        Task UpdateFamilyMemberAsync(int familyMemberId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email, string? role);

        Task<FamilyMember> GetFamilyMemberByIdAsync(int familyMemberId);
        Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId);

        Task<string> GeneratePrintDataAsync(int familyId);
    }
}
