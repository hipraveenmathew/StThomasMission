using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<Student>> GetByGradeAsync(string grade)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Where(s => s.Grade == grade && s.Status != Core.Enums.StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByGroupIdAsync(int groupId)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Where(s => s.GroupId == groupId && s.Status != Core.Enums.StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetAsync(Expression<Func<Student, bool>> predicate)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentIdAsync(int studentId)
        {
            return await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByStudentIdAsync(int studentId)
        {
            return await _context.Assessments
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }

        public IQueryable<Student> GetQueryable(Expression<Func<Student, bool>> predicate)
        {
            return _context.Students
                .Include(s => s.FamilyMember)
                .Include(s => s.Group)
                .Where(predicate)
                .AsQueryable();
        }
        public async Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId)
        {
            return await _context.Students
                .Include(s => s.FamilyMember)
                .Where(s => s.FamilyMember.FamilyId == familyId && s.Status != StudentStatus.Deleted)
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