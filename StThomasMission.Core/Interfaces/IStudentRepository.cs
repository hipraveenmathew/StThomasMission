using StThomasMission.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetByGroupIdAsync(int groupId);
        Task<IEnumerable<Student>> GetAsync(Expression<Func<Student, bool>> predicate);
        Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<Attendance>> GetAttendanceByStudentIdAsync(int studentId);
        Task<IEnumerable<Assessment>> GetAssessmentsByStudentIdAsync(int studentId);
        IQueryable<Student> GetQueryable(Expression<Func<Student, bool>> predicate);
        IQueryable<Attendance> GetAttendanceQueryable(Expression<Func<Attendance, bool>> predicate);
    }
}