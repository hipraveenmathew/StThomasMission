using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;

namespace StThomasMission.Infrastructure.Repositories
{
    public class AssessmentRepository : IAssessmentRepository
    {
        private readonly StThomasMissionDbContext _context;

        public AssessmentRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        public async Task<Assessment> AddAsync(Assessment assessment)
        {
            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();
            return assessment;
        }

        public async Task<IEnumerable<Assessment>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Assessments
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }
    }
}