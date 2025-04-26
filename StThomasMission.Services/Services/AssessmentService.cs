using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly IAuditService _auditService;

        public AssessmentService(IUnitOfWork unitOfWork, IStudentService studentService, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _auditService = auditService;
        }

        public async Task AddAssessmentAsync(int studentId, string name, double marks, double totalMarks, DateTime date, AssessmentType type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Assessment name is required.", nameof(name));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Assessment date cannot be in the future.", nameof(date));
            if (marks < 0 || marks > totalMarks)
                throw new ArgumentException("Marks must be between 0 and total marks.", nameof(marks));
            if (totalMarks <= 0)
                throw new ArgumentException("Total marks must be positive.", nameof(totalMarks));

            await _studentService.GetStudentByIdAsync(studentId);

            var assessment = new Assessment
            {
                StudentId = studentId,
                Name = name,
                Marks = marks,
                TotalMarks = totalMarks,
                Date = date,
                Type = type,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Assessments.AddAsync(assessment);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Assessment), assessment.Id.ToString(), $"Added assessment: {name} for student {studentId}");
        }

        public async Task UpdateAssessmentAsync(int assessmentId, string name, double marks, double totalMarks, DateTime date, AssessmentType type)
        {
            var assessment = await GetAssessmentByIdAsync(assessmentId);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Assessment name is required.", nameof(name));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Assessment date cannot be in the future.", nameof(date));
            if (marks < 0 || marks > totalMarks)
                throw new ArgumentException("Marks must be between 0 and total marks.", nameof(marks));
            if (totalMarks <= 0)
                throw new ArgumentException("Total marks must be positive.", nameof(totalMarks));

            assessment.Name = name;
            assessment.Marks = marks;
            assessment.TotalMarks = totalMarks;
            assessment.Date = date;
            assessment.Type = type;
            assessment.UpdatedBy = "System";
            assessment.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Assessments.UpdateAsync(assessment);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Assessment), assessmentId.ToString(), $"Updated assessment: {name}");
        }

        public async Task DeleteAssessmentAsync(int assessmentId)
        {
            var assessment = await GetAssessmentByIdAsync(assessmentId);

            await _unitOfWork.Assessments.DeleteAsync(assessmentId);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Assessment), assessmentId.ToString(), $"Deleted assessment: {assessment.Name}");
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByStudentAsync(int studentId, AssessmentType? type = null)
        {
            await _studentService.GetStudentByIdAsync(studentId);
            return await _unitOfWork.Assessments.GetAssessmentsByStudentIdAsync(studentId, type);
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));

            return await _unitOfWork.Assessments.GetByGradeAsync(grade, startDate, endDate);
        }

        private async Task<Assessment> GetAssessmentByIdAsync(int assessmentId)
        {
            var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
            if (assessment == null)
                throw new ArgumentException("Assessment not found.", nameof(assessmentId));
            return assessment;
        }
    }
}