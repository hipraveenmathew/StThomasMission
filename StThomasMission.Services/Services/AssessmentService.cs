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
    /// Service for managing student assessments.
    /// </summary>
    public class AssessmentService : IAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatechismService _catechismService;

        public AssessmentService(IUnitOfWork unitOfWork, ICatechismService catechismService)
        {
            _unitOfWork = unitOfWork;
            _catechismService = catechismService;
        }

        public async Task AddAssessmentAsync(int studentId, string name, DateTime date, int marks, int totalMarks, AssessmentType type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Assessment name is required.", nameof(name));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Assessment date cannot be in the future.", nameof(date));
            if (marks < 0 || marks > totalMarks)
                throw new ArgumentException("Marks must be between 0 and total marks.", nameof(marks));
            if (totalMarks <= 0)
                throw new ArgumentException("Total marks must be positive.", nameof(totalMarks));

            await _catechismService.GetStudentByIdAsync(studentId); // Validates student exists

            var assessment = new Assessment
            {
                StudentId = studentId,
                Name = name,
                Date = date,
                Marks = marks,
                TotalMarks = totalMarks,
                Type = type
            };

            await _unitOfWork.Assessments.AddAsync(assessment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAssessmentAsync(int assessmentId, string name, DateTime date, int marks, int totalMarks, AssessmentType type)
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
            assessment.Date = date;
            assessment.Marks = marks;
            assessment.TotalMarks = totalMarks;
            assessment.Type = type;

            await _unitOfWork.Assessments.UpdateAsync(assessment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAssessmentAsync(int assessmentId)
        {
            var assessment = await GetAssessmentByIdAsync(assessmentId);
            await _unitOfWork.Assessments.DeleteAsync(assessment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Assessment> GetAssessmentByIdAsync(int assessmentId)
        {
            var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
            if (assessment == null)
                throw new ArgumentException("Assessment not found.", nameof(assessmentId));
            return assessment;
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByStudentIdAsync(int studentId)
        {
            await _catechismService.GetStudentByIdAsync(studentId); // Validates student exists
            return await _unitOfWork.Assessments.GetByStudentIdAsync(studentId);
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByGradeAsync(string grade, int academicYear)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));

            var students = await _catechismService.GetStudentsByGradeAsync(grade);
            var studentIds = students.Where(s => s.AcademicYear == academicYear).Select(s => s.Id);
            var assessments = await _unitOfWork.Assessments.GetAllAsync();
            return assessments.Where(a => studentIds.Contains(a.StudentId));
        }
    }
}