using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly StThomasMissionDbContext _context;

        public StudentRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        // IRepository<Student> Methods

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Attendances)
                .Include(s => s.Assessments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .ToListAsync();
        }

        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Student student)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await GetByIdAsync(id);
            if (student == null)
                throw new ArgumentException($"Student with ID {id} not found.", nameof(id));

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }

        // IStudentRepository Specific Methods

        public async Task<IEnumerable<Student>> GetByGradeAsync(string grade)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Where(s => s.Grade == grade)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId)
        {
            var familyMemberIds = await _context.FamilyMembers
                .Where(fm => fm.FamilyId == familyId)
                .Select(fm => fm.Id)
                .ToListAsync();

            return await _context.Students
                .Include(s => s.FamilyMember)
                .Where(s => familyMemberIds.Contains(s.FamilyMemberId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByGroupAsync(string group)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Where(s => s.Group == group)
                .ToListAsync();
        }
    }
}
