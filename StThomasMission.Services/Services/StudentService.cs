using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    /// <summary>
    /// Service for managing student enrollment and updates.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly ICatechismService _catechismService;

        public StudentService(IUnitOfWork unitOfWork, IFamilyMemberService familyMemberService, ICatechismService catechismService)
        {
            _unitOfWork = unitOfWork;
            _familyMemberService = familyMemberService;
            _catechismService = catechismService;
        }

        public async Task EnrollStudentAsync(int familyMemberId, string grade, int academicYear, string? group, string? studentOrganisation)
        {
            await _familyMemberService.GetFamilyMemberByIdAsync(familyMemberId); // Validates family member exists

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));
            if (!string.IsNullOrEmpty(group))
            {
                var existingGroups = (await _unitOfWork.Students.GetAllAsync())
                    .Where(s => !string.IsNullOrEmpty(s.Group))
                    .Select(s => s.Group)
                    .Distinct()
                    .ToList();
                if (!existingGroups.Contains(group))
                    throw new ArgumentException("Group does not exist in the system.", nameof(group));
            }

            var existingStudent = (await _unitOfWork.Students.GetAllAsync())
                .FirstOrDefault(s => s.FamilyMemberId == familyMemberId && s.AcademicYear == academicYear);
            if (existingStudent != null)
                throw new InvalidOperationException("Student is already enrolled for this academic year.");

            var student = new Student
            {
                FamilyMemberId = familyMemberId,
                Grade = grade,
                AcademicYear = academicYear,
                Group = group,
                StudentOrganisation = studentOrganisation,
                Status = StudentStatus.Active
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateStudentAsync(int studentId, string grade, string? group, string? studentOrganisation, StudentStatus status, string? migratedTo)
        {
            var student = await GetStudentByIdAsync(studentId); // Validates student exists

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (!string.IsNullOrEmpty(group))
            {
                var existingGroups = (await _unitOfWork.Students.GetAllAsync())
                    .Where(s => !string.IsNullOrEmpty(s.Group))
                    .Select(s => s.Group)
                    .Distinct()
                    .ToList();
                if (!existingGroups.Contains(group))
                    throw new ArgumentException("Group does not exist in the system.", nameof(group));
            }
            if (status == StudentStatus.Migrated && string.IsNullOrEmpty(migratedTo))
                throw new ArgumentException("MigratedTo is required for migrated status.", nameof(migratedTo));

            student.Grade = grade;
            student.Group = group;
            student.StudentOrganisation = studentOrganisation;
            student.Status = status;
            student.MigratedTo = migratedTo;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkStudentAsDeletedAsync(int studentId)
        {
            var student = await GetStudentByIdAsync(studentId); // Validates student exists

            // Assuming a soft delete by setting Status to a specific value or marking as deleted
            student.Status = StudentStatus.Deleted; // Adjust if your Student entity has a different property (e.g., IsDeleted)

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
                throw new ArgumentException("Student not found.", nameof(studentId));
            return student;
        }

        public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));

            var students = await _unitOfWork.Students.FindAsync(s => s.Grade == grade);
            return students;
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(string group)
        {
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("Group is required.", nameof(group));

            var students = await _unitOfWork.Students.FindAsync(s => s.Group == group);
            if (!students.Any())
                throw new ArgumentException("No students found for the specified group.", nameof(group));

            return students;
        }

        public async Task MarkPassFailAsync(int studentId, bool hasPassed)
        {
            var student = await GetStudentByIdAsync(studentId); // Validates student exists

            // Assuming StudentStatus has Pass/Fail states; adjust if your system uses a different property
            student.Status = hasPassed ? StudentStatus.Passed : StudentStatus.Failed;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkStudentAsInactiveAsync(int studentId)
        {
            var student = await GetStudentByIdAsync(studentId); // Validates student exists

            student.Status = StudentStatus.Inactive;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task PromoteStudentsAsync(string grade, int academicYear)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));

            var students = (await _unitOfWork.Students.FindAsync(s => s.Grade == grade && s.AcademicYear == academicYear))
                .Where(s => s.Status == StudentStatus.Active || s.Status == StudentStatus.Passed);

            foreach (var student in students)
            {
                // Extract the year number from the grade (e.g., "Year 1" -> 1)
                if (!int.TryParse(Regex.Match(student.Grade, @"\d+").Value, out int currentYear))
                    continue;

                // Promote to the next grade (e.g., "Year 1" -> "Year 2")
                string nextGrade = $"Year {currentYear + 1}";

                // Check if the next grade is valid (e.g., not exceeding max grade)
                if (!Regex.IsMatch(nextGrade, @"^Year \d{1,2}$"))
                {
                    // If promotion exceeds max grade, mark as Graduated
                    student.Status = StudentStatus.Graduated;
                }
                else
                {
                    student.Grade = nextGrade;
                    student.AcademicYear = academicYear + 1; // Move to next academic year
                    student.Status = StudentStatus.Active;
                }

                await _unitOfWork.Students.UpdateAsync(student);
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}