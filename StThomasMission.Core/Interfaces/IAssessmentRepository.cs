using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Repository for Assessment-specific operations.
    /// </summary>
    public interface IAssessmentRepository : IRepository<Assessment>
    {
        Task<IEnumerable<Assessment>> GetByStudentIdAsync(int studentId, bool? isMajor = null);
        Task<IEnumerable<Assessment>> GetByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null);
    }
}
