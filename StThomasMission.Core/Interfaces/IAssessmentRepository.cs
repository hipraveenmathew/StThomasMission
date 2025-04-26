using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAssessmentRepository : IRepository<Assessment>
    {
        Task<IEnumerable<Assessment>> GetAssessmentsByStudentIdAsync(int studentId, AssessmentType? type = null);
        Task<IEnumerable<Assessment>> GetByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Assessment>> GetAsync(Expression<Func<Assessment, bool>> predicate);
    }
}