using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class StudentGroupActivityRepository : Repository<StudentGroupActivity>, IStudentGroupActivityRepository
    {
        public StudentGroupActivityRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<StudentGroupActivityDto>> GetByStudentIdAsync(int studentId)
        {
            // Global query filters will handle soft-deleted students.
            return await _dbSet
                .AsNoTracking()
                .Where(sga => sga.StudentId == studentId)
                .Select(sga => new StudentGroupActivityDto
                {
                    Id = sga.Id,
                    StudentId = sga.StudentId,
                    StudentFullName = sga.Student.FamilyMember.FullName,
                    GroupActivityId = sga.GroupActivityId,
                    ActivityName = sga.GroupActivity.Name,
                    ParticipationDate = sga.ParticipationDate,
                    PointsEarned = sga.PointsEarned,
                    Remarks = sga.Remarks
                })
                .OrderByDescending(dto => dto.ParticipationDate)
                .ToListAsync();
        }

        public async Task<List<int>> GetParticipantsByIdsAsync(int groupActivityId, List<int> studentIds)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sga => sga.GroupActivityId == groupActivityId && studentIds.Contains(sga.StudentId))
                .Select(sga => sga.StudentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentGroupActivityDto>> GetByGroupActivityIdAsync(int groupActivityId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(sga => sga.GroupActivityId == groupActivityId)
                .Select(sga => new StudentGroupActivityDto
                {
                    Id = sga.Id,
                    StudentId = sga.StudentId,
                    StudentFullName = sga.Student.FamilyMember.FullName,
                    GroupActivityId = sga.GroupActivityId,
                    ActivityName = sga.GroupActivity.Name,
                    ParticipationDate = sga.ParticipationDate,
                    PointsEarned = sga.PointsEarned,
                    Remarks = sga.Remarks
                })
                .OrderBy(dto => dto.StudentFullName)
                .ToListAsync();
        }
    }
}