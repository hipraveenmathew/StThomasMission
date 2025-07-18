using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
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

        public async Task<StudentDetailDto> EnrollStudentAsync(EnrollStudentRequest request, string userId)
        {
            if (await _unitOfWork.FamilyMembers.GetByIdAsync(request.FamilyMemberId) == null)
            {
                throw new NotFoundException(nameof(FamilyMember), request.FamilyMemberId);
            }

            var student = new Student
            {
                FamilyMemberId = request.FamilyMemberId,
                AcademicYear = request.AcademicYear,
                GradeId = request.GradeId,
                GroupId = request.GroupId,
                StudentOrganisation = request.StudentOrganisation,
                Status = StudentStatus.Active,
                CreatedBy = userId
            };

            var newStudent = await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Enroll", nameof(Student), newStudent.Id.ToString(), $"Enrolled student into grade ID {request.GradeId}.");

            return (await _unitOfWork.Students.GetStudentDetailAsync(newStudent.Id))!;
        }

        public async Task UpdateStudentDetailsAsync(int studentId, UpdateStudentRequest request, string userId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new NotFoundException(nameof(Student), studentId);

            student.GradeId = request.GradeId;
            student.GroupId = request.GroupId;
            student.StudentOrganisation = request.StudentOrganisation;
            student.Status = request.Status;
            student.MigratedTo = request.Status == StudentStatus.Migrated ? request.MigratedTo : null;
            student.UpdatedBy = userId;
            student.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(Student), studentId.ToString(), "Updated student details.");
        }

        public async Task MarkStudentsAsPassedOrFailAsync(int gradeId, IEnumerable<StudentPassFailRequest> results, string userId)
        {
            var academicYear = DateTime.UtcNow.Year;
            foreach (var result in results)
            {
                var record = await _unitOfWork.StudentAcademicRecords.GetByStudentAndYearAsync(result.StudentId, academicYear);

                if (record != null)
                {
                    // This logic would be in the repository for an update, but for a service, we get the entity then update it.
                    var entityToUpdate = await _unitOfWork.StudentAcademicRecords.GetByIdAsync(record.Id);
                    if (entityToUpdate == null) continue;

                    entityToUpdate.Passed = result.HasPassed;
                    entityToUpdate.Remarks = result.Remarks;
                    await _unitOfWork.StudentAcademicRecords.UpdateAsync(entityToUpdate);
                }
                else
                {
                    var newRecord = new StudentAcademicRecord
                    {
                        StudentId = result.StudentId,
                        GradeId = gradeId,
                        AcademicYear = academicYear,
                        Passed = result.HasPassed,
                        Remarks = result.Remarks,
                        CreatedBy = userId
                    };
                    await _unitOfWork.StudentAcademicRecords.AddAsync(newRecord);
                }
            }
            await _unitOfWork.CompleteAsync();
            await _auditService.LogActionAsync(userId, "MarkPassFail", nameof(StudentAcademicRecord), gradeId.ToString(), $"Marked Pass/Fail status for students in grade ID {gradeId}.");
        }

        // This is the corrected version of the method
        public async Task PromoteStudentsInGradeAsync(int gradeId, string userId)
        {
            int currentYear = DateTime.UtcNow.Year;
            var studentsToPromote = await _unitOfWork.StudentAcademicRecords.GetRecordsByGradeAndYearAsync(gradeId, currentYear);
            var passedStudentIds = studentsToPromote.Where(s => s.Passed).Select(s => s.StudentId).ToList();

            if (!passedStudentIds.Any()) return; // No students to promote

            // Corrected Line: Use the new, specific repository method
            var students = await _unitOfWork.Students.GetByIdsAsync(passedStudentIds);
            var grades = await _unitOfWork.Grades.GetGradesInOrderAsync();

            var currentGrade = grades.FirstOrDefault(g => g.Id == gradeId);
            if (currentGrade == null) throw new NotFoundException("Current Grade", gradeId);

            var nextGrade = grades.FirstOrDefault(g => g.Order == currentGrade.Order + 1);

            foreach (var student in students)
            {
                if (nextGrade == null) // This is the final grade
                {
                    student.Status = StudentStatus.Graduated;
                }
                else
                {
                    student.GradeId = nextGrade.Id;
                    student.AcademicYear = currentYear + 1;
                }
                student.UpdatedBy = userId;
                student.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Students.UpdateAsync(student);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Promote", nameof(Student), gradeId.ToString(), $"Promoted {passedStudentIds.Count} students from grade ID {gradeId}.");
        }
    }
}