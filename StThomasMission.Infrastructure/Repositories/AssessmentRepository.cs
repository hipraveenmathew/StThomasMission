using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class AssessmentRepository : Repository<Assessment>, IAssessmentRepository
    {
        public AssessmentRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<AssessmentDto>> GetAssessmentsByStudentIdAsync(int studentId, AssessmentType? type = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(a => a.StudentId == studentId);

            if (type.HasValue)
            {
                query = query.Where(a => a.Type == type.Value);
            }

            return await query
                .Select(a => new AssessmentDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Date = a.Date,
                    Type = a.Type,
                    Marks = a.Marks,
                    TotalMarks = a.TotalMarks,
                    Percentage = a.Percentage,
                    Remarks = a.Remarks
                })
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssessmentGradeViewDto>> GetAssessmentsByGradeAsync(int gradeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(a => a.Student.GradeId == gradeId);

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Date.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Date.Date <= endDate.Value.Date);
            }

            var dtoQuery = query.Select(a => new AssessmentGradeViewDto
            {
                AssessmentId = a.Id,
                AssessmentName = a.Name,
                AssessmentDate = a.Date,
                StudentId = a.StudentId,
                StudentFullName = a.Student.FamilyMember.FullName,
                Marks = a.Marks,
                TotalMarks = a.TotalMarks
            });

            return await dtoQuery
                .OrderByDescending(a => a.AssessmentDate)
                .ThenBy(a => a.StudentFullName)
                .ToListAsync();
        }
    }
}