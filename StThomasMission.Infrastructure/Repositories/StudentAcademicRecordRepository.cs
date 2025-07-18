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
    public class StudentAcademicRecordRepository : Repository<StudentAcademicRecord>, IStudentAcademicRecordRepository
    {
        public StudentAcademicRecordRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<StudentAcademicRecordDto?> GetByStudentAndYearAsync(int studentId, int academicYear)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.StudentId == studentId && r.AcademicYear == academicYear)
                .Select(r => new StudentAcademicRecordDto
                {
                    Id = r.Id,
                    AcademicYear = r.AcademicYear,
                    GradeName = r.Grade.Name,
                    Passed = r.Passed,
                    Remarks = r.Remarks
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<StudentAcademicRecordDto>> GetByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.StudentId == studentId)
                .Select(r => new StudentAcademicRecordDto
                {
                    Id = r.Id,
                    AcademicYear = r.AcademicYear,
                    GradeName = r.Grade.Name,
                    Passed = r.Passed,
                    Remarks = r.Remarks
                })
                .OrderBy(r => r.AcademicYear)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassAcademicSummaryDto>> GetRecordsByGradeAndYearAsync(int gradeId, int academicYear)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.GradeId == gradeId && r.AcademicYear == academicYear)
                .Select(r => new ClassAcademicSummaryDto
                {
                    StudentId = r.StudentId,
                    StudentFullName = r.Student.FamilyMember.FullName,
                    AcademicYear = r.AcademicYear,
                    GradeName = r.Grade.Name,
                    Passed = r.Passed,
                    Remarks = r.Remarks
                })
                .OrderBy(r => r.StudentFullName)
                .ToListAsync();
        }
    }
}