using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyService
    {
        Task<Family> RegisterFamilyAsync(string familyName, string ward, bool isRegistered, string? churchRegistrationNumber, string? temporaryId);
        Task<FamilyMember> AddFamilyMemberAsync(int familyId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email);
        Task<Student> EnrollStudentAsync(int familyMemberId, string grade, int academicYear, string group, string? studentOrganisation);
        Task<IEnumerable<Family>> GetAllFamiliesAsync();
    }
}