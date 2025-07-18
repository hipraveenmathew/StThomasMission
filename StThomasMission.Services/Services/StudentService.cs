using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public StudentService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<StudentDetailDto> GetStudentDetailsAsync(int studentId)
        {
            var studentDto = await _unitOfWork.Students.GetStudentDetailAsync(studentId);
            if (studentDto == null)
            {
                throw new NotFoundException(nameof(Student), studentId);
            }
            return studentDto;
        }

        public async Task<IPaginatedList<StudentSummaryDto>> SearchStudentsAsync(int pageNumber, int pageSize, string? searchTerm = null, int? gradeId = null, int? groupId = null, StudentStatus? status = null)
        {
            return await _unitOfWork.Students.SearchStudentsPaginatedAsync(pageNumber, pageSize, searchTerm, gradeId, groupId, status);
        }

        public async Task ChangeStudentStatusAsync(int studentId, StudentStatus newStatus, string userId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new NotFoundException(nameof(Student), studentId);
            }

            var oldStatus = student.Status;
            student.Status = newStatus;
            student.UpdatedBy = userId;
            student.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "ChangeStatus", nameof(Student), studentId.ToString(), $"Changed student status from {oldStatus} to {newStatus}.");
        }

        public async Task MigrateStudentAsync(int studentId, string migratedTo, string userId)
        {
            if (string.IsNullOrWhiteSpace(migratedTo))
            {
                throw new ArgumentException("Migration destination cannot be empty.", nameof(migratedTo));
            }

            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new NotFoundException(nameof(Student), studentId);
            }

            student.Status = StudentStatus.Migrated;
            student.MigratedTo = migratedTo;
            student.UpdatedBy = userId;
            student.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Migrate", nameof(Student), studentId.ToString(), $"Migrated student to {migratedTo}.");
        }
    }
}