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
        Task<Family> RegisterFamilyAsync(Family family);
        Task<Family> GetFamilyByIdAsync(int familyId);
        Task<FamilyMember> GetFamilyMemberByUserIdAsync(string userId);
        Task<Family?> GetByChurchRegistrationNumberAsync(string churchRegistrationNumber);
        Task<Family?> GetByTemporaryIdAsync(string temporaryId);
        Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId);
        Task EnrollStudentAsync(int familyMemberId, string grade, int academicYear, int groupId, string? studentOrganisation);
        Task UpdateFamilyAsync(Family family); // Changed to FamilyStatus
        Task<byte[]> GenerateRegistrationSlipAsync(int familyId);
        Task ConvertTemporaryIdToChurchIdAsync(int familyId);
        Task<string> NewChurchIdAsync();
        IQueryable<Family> GetFamiliesQueryable(string? searchString = null, string? ward = null, FamilyStatus? status = null);
        Task TransitionChildToNewFamilyAsync(int familyMemberId, string newFamilyName, int newWardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId);
        Task AddFamilyMemberAsync(FamilyMember familyMember);
    }
}