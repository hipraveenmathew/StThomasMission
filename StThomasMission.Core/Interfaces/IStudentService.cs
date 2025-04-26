using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentService
    {
        Task<Student> GetStudentByIdAsync(int studentId);
        Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetStudentsByGroupIdAsync(int groupId);
        Task EnrollStudentAsync(int familyMemberId, string grade, int academicYear, int groupId, string studentOrganisation);
        Task UpdateStudentAsync(int studentId, string grade, int groupId, string studentOrganisation, StudentStatus status, string migratedTo);
        Task MarkPassFailAsync(int studentId, int academicYear, double passThreshold = 50.0, string remarks = null);
        Task DeleteStudentAsync(int studentId);
        Task MarkStudentAsInactiveAsync(int studentId);
        Task PromoteStudentsAsync(string grade, int academicYear);
    }
}