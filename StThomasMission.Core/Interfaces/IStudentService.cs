using StThomasMission.Core.DTOs;
using StThomasMission.Core.Enums;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentService
    {
        Task<StudentDetailDto> GetStudentDetailsAsync(int studentId);

        Task<IPaginatedList<StudentSummaryDto>> SearchStudentsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? gradeId = null,
            int? groupId = null,
            StudentStatus? status = null);

        Task ChangeStudentStatusAsync(int studentId, StudentStatus newStatus, string userId);

        Task MigrateStudentAsync(int studentId, string migratedTo, string userId);
    }
}