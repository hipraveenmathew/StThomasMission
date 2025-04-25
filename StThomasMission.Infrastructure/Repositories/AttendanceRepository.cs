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
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        private readonly StThomasMissionDbContext _context;

        public AttendanceRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Attendances
                .AsNoTracking()
                .Where(a => a.StudentId == studentId && a.Student.Status != StudentStatus.Deleted);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByGradeAsync(string grade, DateTime date)
        {
            return await _context.Attendances
                .AsNoTracking()
                .Include(a => a.Student)
                .Where(a => a.Student.Grade == grade && a.Date.Date == date.Date && a.Student.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByGradeAndStatusAsync(string grade, DateTime date, StudentStatus status)
        {
            return await _context.Attendances
                .AsNoTracking()
                .Include(a => a.Student)
                .Where(a => a.Student.Grade == grade && a.Date.Date == date.Date && a.Student.Status == status)
                .ToListAsync();
        }
    }
}