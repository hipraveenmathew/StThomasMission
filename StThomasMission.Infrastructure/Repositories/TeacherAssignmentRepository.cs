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
    public class TeacherAssignmentRepository : Repository<TeacherAssignment>, ITeacherAssignmentRepository
    {
        public TeacherAssignmentRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<TeacherAssignmentDto>> GetAssignmentsByYearAsync(int academicYear)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ta => ta.AcademicYear == academicYear)
                .Select(ta => new TeacherAssignmentDto
                {
                    UserId = ta.UserId,
                    TeacherFullName = ta.User.FullName,
                    GradeId = ta.GradeId,
                    GradeName = ta.Grade.Name,
                    GroupId = ta.GroupId,
                    GroupName = ta.Group != null ? ta.Group.Name : null,
                    AcademicYear = ta.AcademicYear
                })
                .OrderBy(dto => dto.GradeName)
                .ToListAsync();
        }

        public async Task<TeacherAssignmentDto?> GetAssignmentForTeacherAsync(string userId, int academicYear)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ta => ta.UserId == userId && ta.AcademicYear == academicYear)
                .Select(ta => new TeacherAssignmentDto
                {
                    UserId = ta.UserId,
                    TeacherFullName = ta.User.FullName,
                    GradeId = ta.GradeId,
                    GradeName = ta.Grade.Name,
                    GroupId = ta.GroupId,
                    GroupName = ta.Group != null ? ta.Group.Name : null,
                    AcademicYear = ta.AcademicYear
                })
                .FirstOrDefaultAsync();
        }
    }
}