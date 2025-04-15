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
    public class AssessmentRepository : IAssessmentRepository
    {
        private readonly StThomasMissionDbContext _context;

        public AssessmentRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Assessment>> GetAllAsync()
        {
            return await _context.Assessments.ToListAsync();
        }

        public async Task<Assessment> GetByIdAsync(int id)
        {
            return await _context.Assessments.FindAsync(id);
        }

        public async Task AddAsync(Assessment assessment)
        {
            await _context.Assessments.AddAsync(assessment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Assessment assessment)
        {
            _context.Assessments.Update(assessment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Assessments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Assessment entity)
        {
            _context.Assessments.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Assessment>> GetByStudentIdAsync(int studentId, bool? isMajor = null)
        {
            var query = _context.Assessments.Where(a => a.StudentId == studentId);

            if (isMajor.HasValue)
            {
                query = query.Where(a => a.Type == (isMajor.Value ? AssessmentType.SemesterExam : AssessmentType.ClassAssessment));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Assessment>> GetByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Assessments
                .Include(a => a.Student)
                .Where(a => a.Student.Grade == grade);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            return await query.ToListAsync();
        }
    }
}
