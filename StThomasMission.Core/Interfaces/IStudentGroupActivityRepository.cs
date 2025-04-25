using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentGroupActivityRepository : IRepository<StudentGroupActivity>
    {
        Task<bool> AnyAsync(Expression<Func<StudentGroupActivity, bool>> predicate);
        Task<IEnumerable<StudentGroupActivity>> GetByStudentIdAsync(int studentId);
    }
}