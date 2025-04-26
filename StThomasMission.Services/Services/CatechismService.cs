using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class CatechismService : ICatechismService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IStudentService _studentService;

        public CatechismService(IUnitOfWork unitOfWork, IAuditService auditService, IStudentService studentService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _studentService = studentService;
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null || student.Status == StudentStatus.Deleted)
                throw new ArgumentException("Student not found.", nameof(studentId));
            return student;
        }

        public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            return await _unitOfWork.Students.GetByGradeAsync(grade);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupIdAsync(int groupId)
        {
            await _unitOfWork.Groups.GetByIdAsync(groupId);
            return await _unitOfWork.Students.GetByGroupIdAsync(groupId);
        }

        public async Task AddStudentAsync(int familyMemberId, int academicYear, string grade, int groupId, string? studentOrganisation)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
            if (familyMember == null)
                throw new ArgumentException("Family member not found.", nameof(familyMemberId));

            await _unitOfWork.Groups.GetByIdAsync(groupId);

            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));

            var existingStudent = await _unitOfWork.Students.GetAsync(s => s.FamilyMemberId == familyMemberId && s.AcademicYear == academicYear);
            if (existingStudent.Any())
                throw new InvalidOperationException("Student is already enrolled for this academic year.");

            var student = new Student
            {
                FamilyMemberId = familyMemberId,
                AcademicYear = academicYear,
                Grade = grade,
                GroupId = groupId,
                StudentOrganisation = studentOrganisation,
                Status = StudentStatus.Active,
                CreatedBy = "System"
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Student), student.Id.ToString(), $"Added student: {familyMember.FullName} for grade {grade}");
        }

        public async Task UpdateStudentAsync(int studentId, string grade, int groupId, string? studentOrganisation, StudentStatus status, string? migratedTo)
        {
            var student = await GetStudentByIdAsync(studentId);

            await _unitOfWork.Groups.GetByIdAsync(groupId);

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (status == StudentStatus.Migrated && string.IsNullOrEmpty(migratedTo))
                throw new ArgumentException("MigratedTo is required for migrated status.", nameof(migratedTo));
            if (status == StudentStatus.Deleted)
                throw new ArgumentException("Use DeleteStudentAsync to mark a student as deleted.", nameof(status));

            student.Grade = grade;
            student.GroupId = groupId;
            student.StudentOrganisation = studentOrganisation;
            student.Status = status;
            student.MigratedTo = status == StudentStatus.Migrated ? migratedTo : null;
            student.UpdatedDate = DateTime.UtcNow;
            student.UpdatedBy = "System";

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Student), studentId.ToString(), $"Updated student: Grade {grade}, Status {status}");
        }

        public async Task MarkPassFailAsync(int studentId, StudentStatus status)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (!Regex.IsMatch(student.Grade, @"^Year \d{1,2}$"))
                throw new InvalidOperationException("Student grade is in invalid format.");

            if (status == StudentStatus.Graduated)
            {
                if (student.Grade != "Year 12")
                    throw new InvalidOperationException("Only Year 12 students can graduate.");
                student.Status = StudentStatus.Graduated;
            }
            else if (status == StudentStatus.Active)
            {
                if (student.Grade == "Year 12")
                {
                    student.Status = StudentStatus.Graduated;
                }
                else
                {
                    int currentYear = int.Parse(student.Grade.Replace("Year ", ""));
                    student.Grade = $"Year {currentYear + 1}";
                    student.AcademicYear += 1;
                    student.Status = StudentStatus.Active;
                }
            }
            else
            {
                throw new ArgumentException("Invalid status for pass/fail operation; use MarkPassFailAsync in IStudentService to record pass/fail.", nameof(status));
            }

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Student), studentId.ToString(), $"Marked student as {status}");
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            var student = await GetStudentByIdAsync(studentId);
            student.Status = StudentStatus.Deleted;
            student.UpdatedDate = DateTime.UtcNow;
            student.UpdatedBy = "System";

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Student), studentId.ToString(), $"Soft-deleted student");
        }

        public async Task PromoteStudentsAsync(string grade, int academicYear)
        {
            var students = await GetStudentsByGradeAsync(grade);
            foreach (var student in students.Where(s => s.AcademicYear == academicYear))
            {
                await MarkPassFailAsync(student.Id, StudentStatus.Active);
            }
        }
    }
}