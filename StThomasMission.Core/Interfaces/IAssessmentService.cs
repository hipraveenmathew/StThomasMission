using StThomasMission.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAssessmentService
    {
        Task<AssessmentDto> GetAssessmentByIdAsync(int assessmentId);

        Task<IEnumerable<AssessmentDto>> GetAssessmentsByStudentIdAsync(int studentId);

        Task<IEnumerable<AssessmentGradeViewDto>> GetAssessmentsByGradeAsync(int gradeId);

        Task<AssessmentDto> CreateAssessmentAsync(CreateAssessmentRequest request, string userId);

        Task UpdateAssessmentAsync(int assessmentId, UpdateAssessmentRequest request, string userId);

        Task DeleteAssessmentAsync(int assessmentId, string userId);
    }
}