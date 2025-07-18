using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.DTOs.Reporting;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<StudentDetailDto?> GetStudentDetailAsync(int studentId)
        {
            // Global query filter handles IsDeleted. Project to a DTO to get all needed data efficiently.
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.Id == studentId)
                .Select(s => new StudentDetailDto
                {
                    Id = s.Id,
                    FamilyMemberId = s.FamilyMemberId,
                    FullName = s.FamilyMember.FullName,
                    AcademicYear = s.AcademicYear,
                    GradeName = s.Grade.Name,
                    GroupName = s.Group != null ? s.Group.Name : null,
                    Status = s.Status,
                    StudentOrganisation = s.StudentOrganisation,
                    MigratedTo = s.MigratedTo,
                    FamilyName = s.FamilyMember.Family.FamilyName,
                    WardName = s.FamilyMember.Family.Ward.Name
                })
                .FirstOrDefaultAsync();
        }
        // Add this new method to the StudentRepository class

        public async Task<StudentSummaryDto?> GetStudentByFamilyMemberId(int familyMemberId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.FamilyMemberId == familyMemberId)
                .Select(s => new StudentSummaryDto
                {
                    Id = s.Id,
                    FullName = s.FamilyMember.FullName,
                    GradeName = s.Grade.Name,
                    GroupName = s.Group != null ? s.Group.Name : null,
                    Status = s.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RecipientContactInfo>> GetParentContactsByGroupIdAsync(int groupId)
        {
            // This query finds all students in a group, then finds the parents in each student's family.
            var contacts = await _dbSet
                .AsNoTracking()
                .Where(s => s.GroupId == groupId)
                .SelectMany(s => s.FamilyMember.Family.FamilyMembers
                    .Where(parent =>
                        (parent.Relation == FamilyMemberRole.Parent || parent.Relation == FamilyMemberRole.Father || parent.Relation == FamilyMemberRole.Mother) &&
                        (!string.IsNullOrEmpty(parent.Email) || !string.IsNullOrEmpty(parent.Contact))
                    )
                    .Select(parent => new RecipientContactInfo
                    {
                        FirstName = parent.FirstName,
                        Email = parent.Email,
                        PhoneNumber = parent.Contact,
                        StudentName = s.FamilyMember.FirstName // Include student's name for message personalization
                    })
                )
                .ToListAsync();

            // Use a Dictionary to return only one contact per unique email/phone, even if they have multiple children in the group
            return contacts
                .GroupBy(c => c.Email ?? c.PhoneNumber)
                .Select(g => g.First());
        }
        public async Task<IEnumerable<Student>> GetByIdsAsync(IEnumerable<int> ids)
        {
            // This method intentionally returns the full, tracked entities because the service
            // layer needs to update them. Therefore, we do NOT use AsNoTracking() here.
            return await _dbSet.Where(s => ids.Contains(s.Id)).ToListAsync();
        }
        public async Task<IEnumerable<StudentSummaryDto>> GetByGradeIdAsync(int gradeId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.GradeId == gradeId)
                .Select(s => new StudentSummaryDto
                {
                    Id = s.Id,
                    FullName = s.FamilyMember.FullName,
                    GradeName = s.Grade.Name,
                    GroupName = s.Group != null ? s.Group.Name : null,
                    Status = s.Status
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentSummaryDto>> GetByGroupIdAsync(int groupId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.GroupId == groupId)
                .Select(s => new StudentSummaryDto
                {
                    Id = s.Id,
                    FullName = s.FamilyMember.FullName,
                    GradeName = s.Grade.Name,
                    GroupName = s.Group!.Name, // Not nullable here as we are filtering by it
                    Status = s.Status
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentSummaryDto>> GetByFamilyIdAsync(int familyId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.FamilyMember.FamilyId == familyId)
                .Select(s => new StudentSummaryDto
                {
                    Id = s.Id,
                    FullName = s.FamilyMember.FullName,
                    GradeName = s.Grade.Name,
                    GroupName = s.Group != null ? s.Group.Name : null,
                    Status = s.Status
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<IPaginatedList<StudentSummaryDto>> SearchStudentsPaginatedAsync(int pageNumber, int pageSize, string? searchTerm = null, int? gradeId = null, int? groupId = null, StudentStatus? status = null)
        {
            var query = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => s.FamilyMember.FullName.Contains(searchTerm));
            }

            if (gradeId.HasValue)
            {
                query = query.Where(s => s.GradeId == gradeId.Value);
            }

            if (groupId.HasValue)
            {
                query = query.Where(s => s.GroupId == groupId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            var dtoQuery = query.Select(s => new StudentSummaryDto
            {
                Id = s.Id,
                FullName = s.FamilyMember.FullName,
                GradeName = s.Grade.Name,
                GroupName = s.Group != null ? s.Group.Name : null,
                Status = s.Status
            });

            return await PaginatedList<StudentSummaryDto>.CreateAsync(
                dtoQuery.OrderBy(s => s.GradeName).ThenBy(s => s.FullName),
                pageNumber,
                pageSize);
        }
        public async Task<StudentReportDto?> GetStudentReportDataAsync(int studentId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.Id == studentId)
                .Select(s => new StudentReportDto
                {
                    FullName = s.FamilyMember.FullName,
                    GradeName = s.Grade.Name,
                    AcademicYear = s.AcademicYear,
                    GroupName = s.Group != null ? s.Group.Name : "N/A",
                    Status = s.Status,
                    Assessments = s.Assessments.Select(a => new AssessmentDto
                    {
                        Name = a.Name,
                        Date = a.Date,
                        Type = a.Type,
                        Marks = a.Marks,
                        TotalMarks = a.TotalMarks
                    }).ToList(),
                    Attendances = s.Attendances.Select(at => new AttendanceDto
                    {
                        Date = at.Date,
                        Description = at.Description,
                        Status = at.Status
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        // This method replaces the previous version in StThomasMission.Infrastructure/Repositories/StudentRepository.cs

        public async Task<ClassReportDto?> GetClassReportDataAsync(int gradeId, int academicYear)
        {
            // Corrected: Directly use the DbContext to find the grade.
            var grade = await _context.Grades.AsNoTracking().FirstOrDefaultAsync(g => g.Id == gradeId);
            if (grade == null) return null;

            var studentsData = await _dbSet
                .AsNoTracking()
                .Where(s => s.GradeId == gradeId && s.AcademicYear == academicYear)
                .Select(s => new ClassReportStudentSummary
                {
                    StudentFullName = s.FamilyMember.FullName,
                    Status = s.Status,
                    TotalAbsences = s.Attendances.Count(a => a.Status == AttendanceStatus.Absent),
                    AverageMark = s.Assessments.Any() ? s.Assessments.Average(a => (a.Marks / a.TotalMarks) * 100) : 0
                })
                .OrderBy(s => s.StudentFullName)
                .ToListAsync();

            return new ClassReportDto
            {
                GradeName = grade.Name,
                AcademicYear = academicYear,
                TotalStudents = studentsData.Count,
                Students = studentsData
            };
        }
    }
}