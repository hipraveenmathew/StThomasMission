using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class StudentAcademicRecordRepository : Repository<StudentAcademicRecord>, IStudentAcademicRecordRepository
    {
        public StudentAcademicRecordRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<StudentAcademicRecord?> GetByStudentAndYearAsync(int studentId, int academicYear)
        {
            return await _context.StudentAcademicRecords
                .FirstOrDefaultAsync(r => r.StudentId == studentId && r.AcademicYear == academicYear);
        }

        public async Task<IEnumerable<StudentAcademicRecord>> GetByStudentIdAsync(int studentId)
        {
            return await _context.StudentAcademicRecords
                .Where(r => r.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentAcademicRecord>> GetAsync(Expression<Func<StudentAcademicRecord, bool>> predicate)
        {
            return await _context.StudentAcademicRecords
                .Where(predicate)
                .ToListAsync();
        }
    }
}