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
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly StThomasMissionDbContext _context;

        public AttendanceRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        // IRepository<Attendance> methods
        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            return await _context.Attendances.ToListAsync();
        }

        public async Task<Attendance> GetByIdAsync(int id)
        {
            return await _context.Attendances.FindAsync(id);
        }

        public async Task AddAsync(Attendance attendance)
        {
            await _context.Attendances.AddAsync(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Attendances.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Attendance entity)
        {
            _context.Attendances.Remove(entity);
            await _context.SaveChangesAsync();
        }

        // IAttendanceRepository specific methods
        public async Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Attendances
                .Where(a => a.StudentId == studentId);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByGradeAsync(string grade, DateTime date)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.Student.Grade == grade && a.Date.Date == date.Date)
                .ToListAsync();
        }
    }
}
