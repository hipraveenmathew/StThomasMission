using StThomasMission.Core.Entities;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentAcademicRecordRepository : IRepository<StudentAcademicRecord>
    {
        Task<StudentAcademicRecord?> GetByStudentAndYearAsync(int studentId, int academicYear);
        Task<IEnumerable<StudentAcademicRecord>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<StudentAcademicRecord>> GetAsync(Expression<Func<StudentAcademicRecord, bool>> predicate);
    }
}