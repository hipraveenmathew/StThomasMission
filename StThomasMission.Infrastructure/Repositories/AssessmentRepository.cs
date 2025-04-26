using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class AssessmentRepository : Repository<Assessment>, IAssessmentRepository
    {
        public AssessmentRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByStudentIdAsync(int studentId, AssessmentType? type = null)
        {
            var query = _context.Assessments
                .Where(a => a.StudentId == studentId);

            if (type.HasValue)
            {
                query = query.Where(a => a.Type == type.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Assessment>> GetByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Assessments
                .Where(a => a.Student.Grade == grade);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Assessment>> GetAsync(Expression<Func<Assessment, bool>> predicate)
        {
            return await _context.Assessments
                .Where(predicate)
                .ToListAsync();
        }
    }
}