using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Attendance>> GetByGradeIdAsync(int gradeId, DateTime date);
        IQueryable<Attendance> GetAttendanceQueryable(Expression<Func<Attendance, bool>> predicate);
    }
}