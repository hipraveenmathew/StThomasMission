using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;

namespace StThomasMission.Services.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FamilyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Family> RegisterFamilyAsync(string familyName, string ward, bool isRegistered, string? churchRegistrationNumber, string? temporaryId)
        {
            var family = new Family
            {
                FamilyName = familyName,
                Ward = ward,
                IsRegistered = isRegistered,
                ChurchRegistrationNumber = churchRegistrationNumber,
                TemporaryID = temporaryId,
                Status = "Active",
                CreatedDate = DateTime.UtcNow
            };
            await _unitOfWork.Families.AddAsync(family);
            await _unitOfWork.CompleteAsync();
            return family;
        }

        public async Task<FamilyMember> AddFamilyMemberAsync(int familyId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null)
            {
                throw new Exception("Family not found");
            }

            var familyMember = new FamilyMember
            {
                FamilyId = familyId,
                FirstName = firstName,
                LastName = lastName,
                Relation = relation,
                DateOfBirth = dateOfBirth,
                Contact = contact,
                Email = email
            };

            // Add the family member to the family's Members collection
            family.Members.Add(familyMember);

            // Update the family in the database
            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            return familyMember;
        }

        public async Task<Student> EnrollStudentAsync(int familyMemberId, string grade, int academicYear, string group, string? studentOrganisation)
        {
            var student = new Student
            {
                FamilyMemberId = familyMemberId,
                Grade = grade,
                AcademicYear = academicYear,
                Group = group,
                StudentOrganisation = studentOrganisation,
                Status = "Active"
            };
            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();
            return student;
        }

        public async Task<IEnumerable<Family>> GetAllFamiliesAsync()
        {
            return await _unitOfWork.Families.GetAllAsync();
        }
    }
}