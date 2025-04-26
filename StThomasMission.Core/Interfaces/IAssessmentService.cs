using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAssessmentService
    {
        Task AddAssessmentAsync(int studentId, string name, double marks, double totalMarks, DateTime date, AssessmentType type);
        Task UpdateAssessmentAsync(int assessmentId, string name, double marks, double totalMarks, DateTime date, AssessmentType type);
        Task DeleteAssessmentAsync(int assessmentId);
        Task<IEnumerable<Assessment>> GetAssessmentsByStudentAsync(int studentId, AssessmentType? type = null);
        Task<IEnumerable<Assessment>> GetAssessmentsByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null);
    }
}