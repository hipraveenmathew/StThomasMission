using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentAcademicRecordRepository : IRepository<StudentAcademicRecord>
    {
        Task<StudentAcademicRecordDto?> GetByStudentAndYearAsync(int studentId, int academicYear);

        Task<IEnumerable<StudentAcademicRecordDto>> GetByStudentIdAsync(int studentId);

        Task<IEnumerable<ClassAcademicSummaryDto>> GetRecordsByGradeAndYearAsync(int gradeId, int academicYear);
    }
}