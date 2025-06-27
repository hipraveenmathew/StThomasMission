using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        // --- UPDATED Method Signature ---
        Task<IEnumerable<Student>> GetByGradeIdAsync(int gradeId);

        // --- Other methods remain ---
        Task<IEnumerable<Student>> GetByGroupIdAsync(int groupId);
        Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<Student>> GetAsync(Expression<Func<Student, bool>> predicate);
        IQueryable<Student> GetQueryable(Expression<Func<Student, bool>> predicate);

        // --- REMOVED Methods ---
        // The following methods belong in their respective repositories (IAttendanceRepository, IAssessmentRepository)
        // Task<IEnumerable<Attendance>> GetAttendanceByStudentIdAsync(int studentId);
        // Task<IEnumerable<Assessment>> GetAssessmentsByStudentIdAsync(int studentId);
    }
}