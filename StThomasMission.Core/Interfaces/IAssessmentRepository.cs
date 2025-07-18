using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IAssessmentRepository : IRepository<Assessment>
    {
        Task<IEnumerable<AssessmentDto>> GetAssessmentsByStudentIdAsync(int studentId, AssessmentType? type = null);

        Task<IEnumerable<AssessmentGradeViewDto>> GetAssessmentsByGradeAsync(int gradeId, DateTime? startDate = null, DateTime? endDate = null);
    }
}