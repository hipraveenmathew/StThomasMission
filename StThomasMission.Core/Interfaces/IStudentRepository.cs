using StThomasMission.Core.DTOs;
using StThomasMission.Core.DTOs.Reporting;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<StudentDetailDto?> GetStudentDetailAsync(int studentId);
        Task<IEnumerable<Student>> GetByIdsAsync(IEnumerable<int> ids);

        Task<IEnumerable<StudentSummaryDto>> GetByGradeIdAsync(int gradeId);
        Task<StudentSummaryDto?> GetStudentByFamilyMemberId(int familyMemberId);

        Task<IEnumerable<StudentSummaryDto>> GetByGroupIdAsync(int groupId);

        Task<IEnumerable<StudentSummaryDto>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<RecipientContactInfo>> GetParentContactsByGroupIdAsync(int groupId);

        Task<IPaginatedList<StudentSummaryDto>> SearchStudentsPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? gradeId = null,
            int? groupId = null,
            StudentStatus? status = null);
        Task<StudentReportDto?> GetStudentReportDataAsync(int studentId);
        Task<ClassReportDto?> GetClassReportDataAsync(int gradeId, int academicYear);
    }
}