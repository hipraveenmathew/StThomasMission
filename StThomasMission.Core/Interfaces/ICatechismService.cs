using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface ICatechismService
    {
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task<IEnumerable<Student>> GetStudentsByGradeIdAsync(int gradeId);
        Task<IEnumerable<Student>> GetStudentsByGroupIdAsync(int groupId);

        Task AddStudentAsync(int familyMemberId, int academicYear, int gradeId, int? groupId, string? studentOrganisation, string createdByUserId);
        Task UpdateStudentAsync(int studentId, int gradeId, int? groupId, string? studentOrganisation, StudentStatus status, string? migratedTo, string updatedByUserId);
        Task PromoteStudentAsync(int studentId, string updatedByUserId);
        Task RevertStudentPromotionAsync(int studentId, string updatedByUserId);
        Task GraduateStudentAsync(int studentId, string updatedByUserId);
        Task BulkPromoteStudentsByGradeAsync(int gradeId, string updatedByUserId);
        Task DeleteStudentAsync(int studentId, string deletedByUserId);
    }
}