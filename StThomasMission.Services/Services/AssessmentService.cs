using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public AssessmentService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<AssessmentDto> GetAssessmentByIdAsync(int assessmentId)
        {
            var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
            if (assessment == null)
            {
                throw new NotFoundException(nameof(Assessment), assessmentId);
            }

            // Map to DTO
            return new AssessmentDto
            {
                Id = assessment.Id,
                StudentId = assessment.StudentId,
                Name = assessment.Name,
                Date = assessment.Date,
                Type = assessment.Type,
                Marks = assessment.Marks,
                TotalMarks = assessment.TotalMarks,
                Percentage = assessment.Percentage,
                Remarks = assessment.Remarks
            };
        }

        public async Task<IEnumerable<AssessmentDto>> GetAssessmentsByStudentIdAsync(int studentId)
        {
            // Verify student exists before proceeding
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new NotFoundException(nameof(Student), studentId);
            }
            return await _unitOfWork.Assessments.GetAssessmentsByStudentIdAsync(studentId);
        }

        public async Task<IEnumerable<AssessmentGradeViewDto>> GetAssessmentsByGradeAsync(int gradeId)
        {
            return await _unitOfWork.Assessments.GetAssessmentsByGradeAsync(gradeId);
        }

        public async Task<AssessmentDto> CreateAssessmentAsync(CreateAssessmentRequest request, string userId)
        {
            // Ensure the student exists
            var student = await _unitOfWork.Students.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                throw new NotFoundException(nameof(Student), request.StudentId);
            }

            var assessment = new Assessment
            {
                StudentId = request.StudentId,
                Name = request.Name,
                Marks = request.Marks,
                TotalMarks = request.TotalMarks,
                Date = request.Date,
                Type = request.Type,
                Remarks = request.Remarks,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            var newAssessment = await _unitOfWork.Assessments.AddAsync(assessment);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Create", nameof(Assessment), newAssessment.Id.ToString(), $"Added assessment '{newAssessment.Name}' for student ID {newAssessment.StudentId}");

            return await GetAssessmentByIdAsync(newAssessment.Id);
        }

        public async Task UpdateAssessmentAsync(int assessmentId, UpdateAssessmentRequest request, string userId)
        {
            var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
            if (assessment == null)
            {
                throw new NotFoundException(nameof(Assessment), assessmentId);
            }

            assessment.Name = request.Name;
            assessment.Marks = request.Marks;
            assessment.TotalMarks = request.TotalMarks;
            assessment.Date = request.Date;
            assessment.Type = request.Type;
            assessment.Remarks = request.Remarks;
            assessment.UpdatedBy = userId;
            assessment.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Assessments.UpdateAsync(assessment);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(Assessment), assessmentId.ToString(), $"Updated assessment '{assessment.Name}'");
        }

        public async Task DeleteAssessmentAsync(int assessmentId, string userId)
        {
            var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
            if (assessment == null)
            {
                throw new NotFoundException(nameof(Assessment), assessmentId);
            }

            assessment.IsDeleted = true;
            assessment.UpdatedBy = userId;
            assessment.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Assessments.UpdateAsync(assessment);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(Assessment), assessmentId.ToString(), $"Soft-deleted assessment: '{assessment.Name}'");
        }
    }
}