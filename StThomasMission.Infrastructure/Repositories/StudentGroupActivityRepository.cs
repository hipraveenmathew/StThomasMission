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
    public class StudentGroupActivityRepository : Repository<StudentGroupActivity>, IStudentGroupActivityRepository
    {
        private readonly StThomasMissionDbContext _context;

        public StudentGroupActivityRepository(StThomasMissionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentGroupActivity>> GetByStudentIdAsync(int studentId)
        {
            return await _context.StudentGroupActivities
                .AsNoTracking()
                .Where(sga => sga.StudentId == studentId && sga.Student.Status != StudentStatus.Deleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentGroupActivity>> GetByGroupActivityIdAsync(int groupActivityId)
        {
            return await _context.StudentGroupActivities
                .AsNoTracking()
                .Where(sga => sga.GroupActivityId == groupActivityId && sga.Student.Status != StudentStatus.Deleted)
                .ToListAsync();
        }
    }
}