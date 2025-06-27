using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(StThomasMissionDbContext context) : base(context) { }

        // --- UPDATED METHOD ---
        // Changed to use gradeId instead of a string to match the schema.
        public async Task<IEnumerable<Student>> GetByGradeIdAsync(int gradeId)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Include(s => s.Grade) // Added Include for Grade
                .Where(s => s.GradeId == gradeId && s.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByGroupIdAsync(int groupId)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Include(s => s.Grade)
                .Where(s => s.GroupId == groupId && s.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Include(s => s.Grade)
                .Where(s => s.FamilyMember.FamilyId == familyId && s.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetAsync(Expression<Func<Student, bool>> predicate)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Include(s => s.Grade)
                .Where(predicate)
                .ToListAsync();
        }

        public IQueryable<Student> GetQueryable(Expression<Func<Student, bool>> predicate)
        {
            return _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Include(s => s.Grade)
                .Where(predicate)
                .AsQueryable();
        }
    }
}