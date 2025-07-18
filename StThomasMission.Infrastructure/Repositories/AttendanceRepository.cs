using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
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
        public AttendanceRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByStudentIdAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(a => a.StudentId == studentId);

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Date.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Date.Date <= endDate.Value.Date);
            }

            return await query
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    Date = a.Date,
                    Description = a.Description,
                    Status = a.Status
                })
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassAttendanceRecordDto>> GetAttendanceForGradeOnDateAsync(int gradeId, DateTime date)
        {
            // The global query filter on Student handles the IsDeleted status automatically.
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.Student.GradeId == gradeId && a.Date.Date == date.Date)
                .Select(a => new ClassAttendanceRecordDto
                {
                    AttendanceId = a.Id,
                    Date = a.Date,
                    Description = a.Description,
                    Status = a.Status,
                    StudentId = a.StudentId,
                    StudentFullName = a.Student.FamilyMember.FullName,
                    Remarks = a.Remarks
                })
                .OrderBy(dto => dto.StudentFullName)
                .ToListAsync();
        }
    }
}