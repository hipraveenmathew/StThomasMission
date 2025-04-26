using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        private readonly StThomasMissionDbContext _context;

        public StudentRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetByGradeAsync(string grade)
        {
            return await _context.Students
                .AsNoTracking()
                .Include(s => s.FamilyMember)
                .Where(s => s.Grade == grade && s.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId)
        {
            var familyMemberIds = await _context.FamilyMembers
                .Where(fm => fm.FamilyId == familyId && fm.Family.Status != FamilyStatus.Deleted)
                .Select(fm => fm.Id)
                .ToListAsync();

            return await _context.Students
                .AsNoTracking()
                .Include(s => s.FamilyMember)
                .Where(s => familyMemberIds.Contains(s.FamilyMemberId) && s.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByGroupIdAsync(int groupId)
        {
            return await _context.Students
                .AsNoTracking()
                .Include(s => s.FamilyMember)
                .Where(s => s.GroupId == groupId && s.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByStatusAsync(StudentStatus status)
        {
            return await _context.Students
                .AsNoTracking()
                .Include(s => s.FamilyMember)
                .Where(s => s.Status == status)
                .ToListAsync();
        }
    }
}