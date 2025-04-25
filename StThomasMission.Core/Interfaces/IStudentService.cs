using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing Student operations.
    /// </summary>
    public interface IStudentService
    {
        Task<Student> GetStudentByIdAsync(int studentId);
        Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetStudentsByGroupAsync(string group);
        Task EnrollStudentAsync(int familyMemberId, string grade, int academicYear, string group, string? studentOrganisation);
        Task UpdateStudentAsync(int studentId, string grade, string? group, string? studentOrganisation, StudentStatus status, string? migratedTo);
        Task MarkPassFailAsync(int studentId, bool passed);
        Task MarkStudentAsDeletedAsync(int studentId);
        Task MarkStudentAsInactiveAsync(int studentId);
        Task PromoteStudentsAsync(string grade, int academicYear);
    }
}