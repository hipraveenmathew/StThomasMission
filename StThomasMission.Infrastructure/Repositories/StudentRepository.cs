using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;

namespace StThomasMission.Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly StThomasMissionDbContext _context;

        public StudentRepository(StThomasMissionDbContext context)
        {
            _context = context;
        }

        public async Task<Student> AddAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Attendances)
                .Include(s => s.Assessments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await GetByIdAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Student>> GetByGradeAsync(string grade)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Where(s => s.Grade == grade)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Where(s => s.FamilyMember.FamilyId == familyId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .ToListAsync();
        }
    }
}