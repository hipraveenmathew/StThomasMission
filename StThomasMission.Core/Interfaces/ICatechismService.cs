using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing Catechism-related student operations.
    /// </summary>
    public interface ICatechismService
    {
        Task<Student> GetStudentByIdAsync(int studentId);
        Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetStudentsByGroupAsync(string group);
        Task AddStudentAsync(int familyMemberId, int academicYear, string grade, string? group, string? studentOrganisation);
        Task UpdateStudentAsync(int studentId, string grade, string? group, string? studentOrganisation, StudentStatus status, string? migratedTo);
        Task MarkPassFailAsync(int studentId, StudentStatus status); // Use StudentStatus to reflect Pass/Fail outcome
        Task DeleteStudentAsync(int studentId);
        Task PromoteStudentsAsync(string grade, int academicYear);
    }
}