using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Student), student.Id.ToString(), $"Enrolled student for grade {grade}");
        }

        public async Task UpdateStudentAsync(int studentId, string grade, int groupId, string studentOrganisation, StudentStatus status, string migratedTo)
        {
            var student = await GetStudentByIdAsync(studentId);

            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (status == StudentStatus.Migrated && string.IsNullOrEmpty(migratedTo))
                throw new ArgumentException("MigratedTo is required for migrated status.", nameof(migratedTo));
            if (status == StudentStatus.Deleted)
                throw new ArgumentException("Use DeleteStudentAsync to mark a student as deleted.", nameof(status));

            await _groupService.GetGroupByIdAsync(groupId);

            student.Grade = grade;
            student.GroupId = groupId;
            student.StudentOrganisation = studentOrganisation;
            student.Status = status;
            student.MigratedTo = status == StudentStatus.Migrated ? migratedTo : null;
            student.UpdatedBy = "System";
            student.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Student), studentId.ToString(), $"Updated student: Grade {grade}, Status {status}");
        }

        public async Task MarkPassFailAsync(int studentId, int academicYear, double passThreshold = 50.0, string remarks = null)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (student.AcademicYear != academicYear)
                throw new InvalidOperationException($"Student is not enrolled in academic year {academicYear}.");

            var assessments = await _unitOfWork.Assessments.GetAssessmentsByStudentIdAsync(studentId);
            assessments = assessments.Where(a => a.Date.Year == academicYear);

            double totalMarks = assessments.Sum(a => a.TotalMarks);
            double obtainedMarks = assessments.Sum(a => a.Marks);
            double percentage = totalMarks > 0 ? (obtainedMarks / totalMarks) * 100 : 0;
            bool passed = percentage >= passThreshold;

            var record = await _unitOfWork.StudentAcademicRecords.GetByStudentAndYearAsync(studentId, academicYear);
            if (record == null)
            {
                record = new StudentAcademicRecord
                {
                    StudentId = studentId,
                    AcademicYear = academicYear,
                    Grade = student.Grade,
                    Passed = passed,
                    Remarks = remarks,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.StudentAcademicRecords.AddAsync(record);
            }
            else
            {
                record.Passed = passed;
                record.Remarks = remarks;
                record.UpdatedBy = "System";
                record.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.StudentAcademicRecords.UpdateAsync(record);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(StudentAcademicRecord), studentId.ToString(), $"Recorded pass/fail for student {studentId} in {academicYear}, Passed: {passed}, Percentage: {percentage:F2}%");
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            var student = await GetStudentByIdAsync(studentId);
            student.Status = StudentStatus.Deleted;
            student.UpdatedBy = "System";
            student.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Student), studentId.ToString(), $"Soft-deleted student");
        }

        public async Task MarkStudentAsInactiveAsync(int studentId)
        {
            var student = await GetStudentByIdAsync(studentId);
            student.Status = StudentStatus.Inactive;
            student.UpdatedBy = "System";
            student.UpdatedDate = DateTime.UtcNow;

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

            var students = await _unitOfWork.Students.GetAsync(s => s.Grade == grade && s.AcademicYear == academicYear && s.Status == StudentStatus.Active);
            if (!students.Any())
                throw new InvalidOperationException($"No students found for grade {grade} in academic year {academicYear}.");

            foreach (var student in students)
            {
                var record = await _unitOfWork.StudentAcademicRecords.GetByStudentAndYearAsync(student.Id, academicYear);
                if (record == null || !record.Passed)
                    continue;

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

                student.UpdatedBy = "System";
                student.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.Students.UpdateAsync(student);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Student), "Multiple", $"Promoted students in grade {grade} for academic year {academicYear}");
        }

        public async Task AddAttendanceAsync(int studentId, DateTime date, string description, bool isPresent)
        {
            var student = await GetStudentByIdAsync(studentId);

            var existingAttendance = await _unitOfWork.Attendances.GetAsync(a => a.StudentId == studentId && a.Date.Date == date.Date);
            if (existingAttendance.Any())
                throw new InvalidOperationException("Attendance already recorded for this student on this date.");

            var attendance = new Attendance
            {
                StudentId = studentId,
                Date = date,
                Status = isPresent ? AttendanceStatus.Present : AttendanceStatus.Absent,
                Description = description
            };

            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Attendance), studentId.ToString(), $"Marked attendance for student {studentId}: {attendance.Status}");
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId)
        {
            return await _unitOfWork.Attendances.GetByStudentIdAsync(studentId);
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByDateAsync(DateTime date)
        {
            return await _unitOfWork.Attendances.GetAsync(a => a.Date.Date == date.Date);
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByStudentAsync(int studentId)
        {
            return await _unitOfWork.Assessments.GetAssessmentsByStudentIdAsync(studentId);
        }

        public IQueryable<Student> GetStudentsQueryable(Expression<Func<Student, bool>> predicate)
        {
            return _unitOfWork.Students.GetQueryable(predicate);
        }

        public IQueryable<Attendance> GetAttendanceQueryable(Expression<Func<Attendance, bool>> predicate)
        {
            return _unitOfWork.Attendances.GetAttendanceQueryable(predicate);
        }
    }
}