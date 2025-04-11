using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface IAssessmentRepository
    {
        Task<Assessment> AddAsync(Assessment assessment);
        Task<IEnumerable<Assessment>> GetByStudentIdAsync(int studentId);
    }
}