using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyService
    {
        Task<Family> RegisterFamilyAsync(string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId);
        Task<Family> GetFamilyByIdAsync(int familyId);
        Task<FamilyMember> GetFamilyMemberByUserIdAsync(string userId);
        Task<Family?> GetByChurchRegistrationNumberAsync(string churchRegistrationNumber);
        Task<Family?> GetByTemporaryIdAsync(string temporaryId);
        Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId);
        Task EnrollStudentAsync(int familyMemberId, string grade, int academicYear, int groupId, string? studentOrganisation);
        Task UpdateFamilyAsync(int familyId, string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, FamilyStatus status, string? migratedTo); // Changed to FamilyStatus        Task<byte[]> GenerateRegistrationSlipAsync(int familyId);
        Task ConvertTemporaryIdToChurchIdAsync(int familyId);
        Task<string> NewChurchIdAsync();
        IQueryable<Family> GetFamiliesQueryable(string? searchString = null, string? ward = null, FamilyStatus? status = null); 
        Task AddFamilyMemberAsync(int familyId, string firstName, string lastName, FamilyMemberRole relation, DateTime dateOfBirth, string? contact, string? email, string? role); 
    }
}