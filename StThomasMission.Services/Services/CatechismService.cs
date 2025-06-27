using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class CatechismService : ICatechismService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public CatechismService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _unitOfWork.Students.GetByIdAsync(studentId);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGradeIdAsync(int gradeId)
        {
            return await _unitOfWork.Students.GetByGradeIdAsync(gradeId);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupIdAsync(int groupId)
        {
            return await _unitOfWork.Students.GetByGroupIdAsync(groupId);
        }

        public async Task AddStudentAsync(int familyMemberId, int academicYear, int gradeId, int? groupId, string? studentOrganisation, string createdByUserId)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
            if (familyMember == null) throw new ArgumentException("Family member not found.", nameof(familyMemberId));

            if (await _unitOfWork.Grades.GetByIdAsync(gradeId) == null) throw new ArgumentException("Invalid Grade ID.", nameof(gradeId));
            if (groupId.HasValue && await _unitOfWork.Groups.GetByIdAsync(groupId.Value) == null) throw new ArgumentException("Invalid Group ID.", nameof(groupId));

            var existingStudent = await _unitOfWork.Students.GetAsync(s => s.FamilyMemberId == familyMemberId && s.Status != StudentStatus.Graduated && s.Status != StudentStatus.Migrated);
            if (existingStudent.Any()) throw new InvalidOperationException("This person is already an active student.");

            var student = new Student
            {
                FamilyMemberId = familyMemberId,
                AcademicYear = academicYear,
                GradeId = gradeId,
                GroupId = groupId,
                StudentOrganisation = studentOrganisation,
                Status = StudentStatus.Active,
                CreatedBy = createdByUserId
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(createdByUserId, "Create", nameof(Student), student.Id.ToString(), $"Enrolled student {familyMember.FullName} into grade {gradeId}.");
        }

        public async Task UpdateStudentAsync(int studentId, int gradeId, int? groupId, string? studentOrganisation, StudentStatus status, string? migratedTo, string updatedByUserId)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));

            if (await _unitOfWork.Grades.GetByIdAsync(gradeId) == null) throw new ArgumentException("Invalid Grade ID.", nameof(gradeId));
            if (groupId.HasValue && await _unitOfWork.Groups.GetByIdAsync(groupId.Value) == null) throw new ArgumentException("Invalid Group ID.", nameof(groupId));

            student.GradeId = gradeId;
            student.GroupId = groupId;
            student.StudentOrganisation = studentOrganisation;
            student.Status = status;
            student.MigratedTo = status == StudentStatus.Migrated ? migratedTo : null;
            student.UpdatedDate = DateTime.UtcNow;
            student.UpdatedBy = updatedByUserId;

            _unitOfWork.Students.Update(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(updatedByUserId, "Update", nameof(Student), studentId.ToString(), $"Updated student details.");
        }

        public async Task PromoteStudentAsync(int studentId, string updatedByUserId)
        {
            var student = await _unitOfWork.Students.GetQueryable(s => s.Id == studentId).Include(s => s.Grade).FirstOrDefaultAsync();
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));
            if (student.Status != StudentStatus.Active) throw new InvalidOperationException("Only active students can be promoted.");

            var nextGrade = await _unitOfWork.Grades.GetQueryable(g => g.Order == student.Grade.Order + 1).FirstOrDefaultAsync();
            if (nextGrade == null)
            {
                // This is the highest grade, so graduate the student
                await GraduateStudentAsync(studentId, updatedByUserId);
                return;
            }

            student.GradeId = nextGrade.Id;
            student.AcademicYear = DateTime.UtcNow.Year; // Set to the current academic year on promotion
            student.UpdatedBy = updatedByUserId;
            student.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Students.Update(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(updatedByUserId, "Promote", nameof(Student), studentId.ToString(), $"Promoted student to grade {nextGrade.Name}.");
        }

        public async Task RevertStudentPromotionAsync(int studentId, string updatedByUserId)
        {
            var student = await _unitOfWork.Students.GetQueryable(s => s.Id == studentId).Include(s => s.Grade).FirstOrDefaultAsync();
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));

            var previousGrade = await _unitOfWork.Grades.GetQueryable(g => g.Order == student.Grade.Order - 1).FirstOrDefaultAsync();
            if (previousGrade == null) throw new InvalidOperationException("Cannot revert promotion for a student in the lowest grade.");

            student.GradeId = previousGrade.Id;
            student.AcademicYear -= 1; // Revert academic year
            student.UpdatedBy = updatedByUserId;
            student.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Students.Update(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(updatedByUserId, "Revert Promotion", nameof(Student), studentId.ToString(), $"Reverted student promotion to grade {previousGrade.Name}.");
        }

        public async Task GraduateStudentAsync(int studentId, string updatedByUserId)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));

            student.Status = StudentStatus.Graduated;
            student.UpdatedBy = updatedByUserId;
            student.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Students.Update(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(updatedByUserId, "Graduate", nameof(Student), studentId.ToString(), $"Graduated student.");
        }

        public async Task BulkPromoteStudentsByGradeAsync(int gradeId, string updatedByUserId)
        {
            var studentsToPromote = await _unitOfWork.Students.GetByGradeIdAsync(gradeId);

            foreach (var student in studentsToPromote.Where(s => s.Status == StudentStatus.Active))
            {
                await PromoteStudentAsync(student.Id, updatedByUserId);
            }
        }

        public async Task DeleteStudentAsync(int studentId, string deletedByUserId)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (student == null) return; // Already gone or never existed

            student.Status = StudentStatus.Deleted;
            student.UpdatedDate = DateTime.UtcNow;
            student.UpdatedBy = deletedByUserId;

            _unitOfWork.Students.Update(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(deletedByUserId, "Delete", nameof(Student), studentId.ToString(), "Soft-deleted student.");
        }
    }
}