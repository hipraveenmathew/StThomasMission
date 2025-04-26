using Microsoft.EntityFrameworkCore;
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
        private readonly StThomasMissionDbContext _context;

        public AssessmentRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Assessment>> GetByStudentIdAsync(int studentId, AssessmentType? type = null)
        {
            var query = _context.Assessments
                .AsNoTracking()
                .Where(a => a.StudentId == studentId && a.Student.Status != StudentStatus.Deleted);

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Assessment>> GetByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Assessments
                .AsNoTracking()
                .Include(a => a.Student)
                .Where(a => a.Student.Grade == grade && a.Student.Status != StudentStatus.Deleted);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            return await query.ToListAsync();
        }
    }
}