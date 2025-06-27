using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Attendances
                .AsNoTracking()
                .Include(a => a.Student) // Added Include for reliable filtering
                .Where(a => a.StudentId == studentId && a.Student.Status != StudentStatus.Deleted);

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Date.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Date.Date <= endDate.Value.Date);
            }

            return await query.ToListAsync();
        }

        // --- UPDATED METHOD ---
        // Changed to use gradeId instead of a string to match the new schema.
        public async Task<IEnumerable<Attendance>> GetByGradeIdAsync(int gradeId, DateTime date)
        {
            return await _context.Attendances
                .AsNoTracking()
                .Include(a => a.Student)
                .Where(a => a.Student.GradeId == gradeId && a.Date.Date == date.Date && a.Student.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public IQueryable<Attendance> GetAttendanceQueryable(Expression<Func<Attendance, bool>> predicate)
        {
            return _context.Attendances
                .Where(predicate)
                .AsQueryable();
        }
    }
}