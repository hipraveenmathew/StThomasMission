
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services;
using System.Text.RegularExpressions;


namespace StThomasMission.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IGroupService _groupService;
        private readonly IAuditService _auditService;

        public StudentService(IUnitOfWork unitOfWork, IFamilyMemberService familyMemberService, IGroupService groupService, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _familyMemberService = familyMemberService;
            _groupService = groupService;
            _auditService = auditService;
        }

        public async Task EnrollStudentAsync(int familyMemberId, string grade, int academicYear, int groupId, string studentOrganisation)
        {
            await _familyMemberService.GetFamilyMemberByIdAsync(familyMemberId);
            await _groupService.GetGroupByIdAsync(groupId);

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));

            var existingStudent = await _unitOfWork.Students.GetAsync(s => s.FamilyMemberId == familyMemberId && s.AcademicYear == academicYear);
            if (existingStudent.Any())
                throw new InvalidOperationException("Student is already enrolled for this academic year.");

            var student = new Student
            {
                FamilyMemberId = familyMemberId,
                Grade = grade,
                AcademicYear = academicYear,
                GroupId = groupId,
                StudentOrganisation = studentOrganisation,
                Status = StudentStatus.Active,
                CreatedBy = "System"
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Student), student.Id.ToString(), $"Enrolled student for grade {grade}");
        }

        public async Task UpdateStudentAsync(int studentId, string grade, int groupId, string studentOrganisation, StudentStatus status, string migratedTo)
        {
            var student = await GetStudentByIdAsync(studentId);

            await _groupService.GetGroupByIdAsync(groupId);

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (status == StudentStatus.Migrated && string.IsNullOrEmpty(migratedTo))
                throw new ArgumentException("MigratedTo is required for migrated status.", nameof(migratedTo));
            if (status == StudentStatus.Deleted)
                throw new ArgumentException("Use DeleteStudentAsync to mark a student as deleted.", nameof(status));

            student.Grade = grade;
            student.GroupId = groupId;
            student.StudentOrganisation = studentOrganisation;
            student.Status = status;
            student.MigratedTo = status == StudentStatus.Migrated ? migratedTo : null;
            student.UpdatedDate = DateTime.UtcNow;
            student.UpdatedBy = "System";

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Student), studentId.ToString(), $"Updated student: Grade {grade}, Status {status}");
        }

        public async Task MarkPassFailAsync(int studentId, int academicYear, double passThreshold = 50.0, string remarks = null)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (student.AcademicYear != academicYear)
                throw new InvalidOperationException($"Student is not enrolled in academic year {academicYear}.");

            // Aggregate assessments for the academic year
            var assessments = await _unitOfWork.Assessments.GetByStudentIdAsync(studentId);
            assessments = assessments.Where(a => a.Date.Year == academicYear);

            double totalMarks = assessments.Sum(a => a.TotalMarks);
            double obtainedMarks = assessments.Sum(a => a.Marks);
            double percentage = totalMarks > 0 ? (obtainedMarks / totalMarks) * 100 : 0;
            bool passed = percentage >= passThreshold;

            // Create or update AssessmentSummary
            var summary = await _unitOfWork.AssessmentSummaries.GetAsync(s => s.StudentId == studentId && s.AcademicYear == academicYear);
            var existingSummary = summary.FirstOrDefault();

            if (existingSummary != null)
            {
                existingSummary.TotalMarks = totalMarks;
                existingSummary.ObtainedMarks = obtainedMarks;
                existingSummary.Passed = passed;
                existingSummary.Grade = student.Grade;
                existingSummary.Remarks = remarks;
                existingSummary.UpdatedAt = DateTime.UtcNow;
                existingSummary.UpdatedBy = "System";

                await _unitOfWork.AssessmentSummaries.UpdateAsync(existingSummary);
            }
            else
            {
                var newSummary = new AssessmentSummary
                {
                    StudentId = studentId,
                    AcademicYear = academicYear,
                    Grade = student.Grade,
                    TotalMarks = totalMarks,
                    ObtainedMarks = obtainedMarks,
                    Passed = passed,
                    Remarks = remarks,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                await _unitOfWork.AssessmentSummaries.AddAsync(newSummary);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(AssessmentSummary), studentId.ToString(), $"Recorded pass/fail for student {studentId} in {academicYear}, Passed: {passed}, Percentage: {percentage:F2}%");
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            var student = await GetStudentByIdAsync(studentId);
            student.Status = StudentStatus.Deleted;
            student.UpdatedDate = DateTime.UtcNow;
            student.UpdatedBy = "System";

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Student), studentId.ToString(), $"Soft-deleted student");
        }

        public async Task MarkStudentAsInactiveAsync(int studentId)
        {
            var student = await GetStudentByIdAsync(studentId);
            student.Status = StudentStatus.Inactive;
            student.UpdatedDate = DateTime.UtcNow;
            student.UpdatedBy = "System";

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Student), studentId.ToString(), $"Marked student as Inactive");
        }

        public async Task PromoteStudentsAsync(string grade, int academicYear)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));

            var summaries = await _unitOfWork.AssessmentSummaries.GetAsync(s => s.Student.AcademicYear == academicYear && s.Student.Grade == grade && s.Passed);
            var studentIds = summaries.Select(s => s.StudentId).ToList();
            var students = await _unitOfWork.Students.GetAsync(s => studentIds.Contains(s.Id) && s.Status == StudentStatus.Active);

            foreach (var student in students)
            {
                if (!int.TryParse(Regex.Match(student.Grade, @"\d+").Value, out int currentYear))
                    continue;

                if (currentYear >= 12)
                {
                    student.Status = StudentStatus.Graduated;
                }
                else
                {
                    student.Grade = $"Year {currentYear + 1}";
                    student.AcademicYear = academicYear + 1;
                    student.Status = StudentStatus.Active;
                }

                student.UpdatedDate = DateTime.UtcNow;
                student.UpdatedBy = "System";

                await _unitOfWork.Students.UpdateAsync(student);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Student), "Multiple", $"Promoted students in grade {grade} for {academicYear}");
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null || student.Status == StudentStatus.Deleted)
                throw new ArgumentException("Student not found.", nameof(studentId));
            return student;
        }

        public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            return await _unitOfWork.Students.GetByGradeAsync(grade);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupIdAsync(int groupId)
        {
            await _groupService.GetGroupByIdAsync(groupId);
            return await _unitOfWork.Students.GetByGroupIdAsync(groupId);
        }
    }
}