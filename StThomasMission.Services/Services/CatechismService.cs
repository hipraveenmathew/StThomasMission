using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class CatechismService : ICatechismService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommunicationService _communicationService;

        public CatechismService(IUnitOfWork unitOfWork, ICommunicationService communicationService)
        {
            _unitOfWork = unitOfWork;
            _communicationService = communicationService;
        }

        // Student Management
        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));
            return student;
        }

        public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$")) throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            return await _unitOfWork.Students.GetByGradeAsync(grade);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(string group)
        {
            if (string.IsNullOrEmpty(group)) throw new ArgumentException("Group is required.", nameof(group));
            return await _unitOfWork.Students.GetByGroupAsync(group);
        }

        public async Task AddStudentAsync(int familyMemberId, int academicYear, string grade, string? group, string? studentOrganisation)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
            if (familyMember == null) throw new ArgumentException("Family member not found.", nameof(familyMemberId));
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year) throw new ArgumentException("Invalid academic year.", nameof(academicYear));
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$")) throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));

            var student = new Student
            {
                FamilyMemberId = familyMemberId,
                AcademicYear = academicYear,
                Grade = grade,
                Group = group,
                StudentOrganisation = studentOrganisation,
                Status = "Active"
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateStudentAsync(int studentId, string grade, string? group, string? studentOrganisation, string status, string? migratedTo)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$")) throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (!new[] { "Active", "Graduated", "Migrated" }.Contains(status)) throw new ArgumentException("Invalid status.", nameof(status));

            student.Grade = grade;
            student.Group = group;
            student.StudentOrganisation = studentOrganisation;
            student.Status = status;
            student.MigratedTo = migratedTo;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkPassFailAsync(int studentId, bool passed)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (!Regex.IsMatch(student.Grade, @"^Year \d{1,2}$")) throw new InvalidOperationException("Student grade is in invalid format.");

            if (passed)
            {
                if (student.Grade == "Year 12")
                {
                    student.Status = "Graduated";
                }
                else
                {
                    int currentYear = int.Parse(student.Grade.Replace("Year ", ""));
                    student.Grade = $"Year {currentYear + 1}";
                    student.AcademicYear += 1;
                }
            }
            // If failed, no action (student remains in current grade)

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task PromoteStudentsAsync(string grade, int academicYear)
        {
            var students = await GetStudentsByGradeAsync(grade);
            foreach (var student in students.Where(s => s.AcademicYear == academicYear))
            {
                await MarkPassFailAsync(student.Id, true);
            }
        }

        // Attendance
        public async Task AddAttendanceAsync(int studentId, DateTime date, string description, bool isPresent)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (string.IsNullOrEmpty(description)) throw new ArgumentException("Description is required.", nameof(description));
            if (date > DateTime.UtcNow) throw new ArgumentException("Attendance date cannot be in the future.", nameof(date));

            var attendance = new Attendance
            {
                StudentId = studentId,
                Date = date,
                Description = description,
                IsPresent = isPresent
            };

            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.CompleteAsync();

            if (!isPresent)
            {
                await _communicationService.SendAbsenteeNotificationAsync(studentId, date);
            }
        }

        public async Task UpdateAttendanceAsync(int attendanceId, bool isPresent, string description)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(attendanceId);
            if (attendance == null) throw new ArgumentException("Attendance record not found.", nameof(attendanceId));
            if (string.IsNullOrEmpty(description)) throw new ArgumentException("Description is required.", nameof(description));

            var previousIsPresent = attendance.IsPresent;
            attendance.IsPresent = isPresent;
            attendance.Description = description;

            await _unitOfWork.Attendances.UpdateAsync(attendance);
            await _unitOfWork.CompleteAsync();

            if (previousIsPresent && !isPresent)
            {
                await _communicationService.SendAbsenteeNotificationAsync(attendance.StudentId, attendance.Date);
            }
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            await GetStudentByIdAsync(studentId); // Validate student exists
            return await _unitOfWork.Attendances.GetByStudentIdAsync(studentId, startDate, endDate);
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByGradeAsync(string grade, DateTime date)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$")) throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            return await _unitOfWork.Attendances.GetByGradeAsync(grade, date);
        }

        // Assessments
        public async Task AddAssessmentAsync(int studentId, string name, int marks, int totalMarks, DateTime date, bool isMajor)
        {
            var student = await GetStudentByIdAsync(studentId);
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Assessment name is required.", nameof(name));
            if (marks < 0) throw new ArgumentException("Marks cannot be negative.", nameof(marks));
            if (totalMarks <= 0) throw new ArgumentException("Total marks must be positive.", nameof(totalMarks));
            if (marks > totalMarks) throw new ArgumentException("Marks cannot exceed total marks.", nameof(marks));
            if (date > DateTime.UtcNow) throw new ArgumentException("Assessment date cannot be in the future.", nameof(date));

            var assessment = new Assessment
            {
                StudentId = studentId,
                Name = name,
                Marks = marks,
                TotalMarks = totalMarks,
                Date = date,
                IsMajor = isMajor
            };

            await _unitOfWork.Assessments.AddAsync(assessment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAssessmentAsync(int assessmentId, string name, int marks, int totalMarks, DateTime date, bool isMajor)
        {
            var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
            if (assessment == null) throw new ArgumentException("Assessment not found.", nameof(assessmentId));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Assessment name is required.", nameof(name));
            if (marks < 0) throw new ArgumentException("Marks cannot be negative.", nameof(marks));
            if (totalMarks <= 0) throw new ArgumentException("Total marks must be positive.", nameof(totalMarks));
            if (marks > totalMarks) throw new ArgumentException("Marks cannot exceed total marks.", nameof(marks));
            if (date > DateTime.UtcNow) throw new ArgumentException("Assessment date cannot be in the future.", nameof(date));

            assessment.Name = name;
            assessment.Marks = marks;
            assessment.TotalMarks = totalMarks;
            assessment.Date = date;
            assessment.IsMajor = isMajor;

            await _unitOfWork.Assessments.UpdateAsync(assessment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByStudentAsync(int studentId, bool? isMajor = null)
        {
            await GetStudentByIdAsync(studentId);
            return await _unitOfWork.Assessments.GetByStudentIdAsync(studentId, isMajor);
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$")) throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            return await _unitOfWork.Assessments.GetByGradeAsync(grade, startDate, endDate);
        }

        // Group Activities
        public async Task AddGroupActivityAsync(string name, string description, DateTime date, string group, int points)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Activity name is required.", nameof(name));
            if (string.IsNullOrEmpty(description)) throw new ArgumentException("Description is required.", nameof(description));
            if (string.IsNullOrEmpty(group)) throw new ArgumentException("Group is required.", nameof(group));
            if (points < 0) throw new ArgumentException("Points cannot be negative.", nameof(points));
            if (date > DateTime.UtcNow.AddDays(365)) throw new ArgumentException("Activity date cannot be more than a year in the future.", nameof(date));

            var groupActivity = new GroupActivity
            {
                Name = name,
                Description = description,
                Date = date,
                Group = group,
                Points = points,
                Status = "Active"
            };

            await _unitOfWork.GroupActivities.AddAsync(groupActivity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateGroupActivityAsync(int groupActivityId, string name, string description, DateTime date, string group, int points, string status)
        {
            var groupActivity = await _unitOfWork.GroupActivities.GetByIdAsync(groupActivityId);
            if (groupActivity == null) throw new ArgumentException("Group activity not found.", nameof(groupActivityId));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Activity name is required.", nameof(name));
            if (string.IsNullOrEmpty(description)) throw new ArgumentException("Description is required.", nameof(description));
            if (string.IsNullOrEmpty(group)) throw new ArgumentException("Group is required.", nameof(group));
            if (points < 0) throw new ArgumentException("Points cannot be negative.", nameof(points));
            if (!new[] { "Active", "Completed", "Cancelled" }.Contains(status)) throw new ArgumentException("Invalid status.", nameof(status));
            if (date > DateTime.UtcNow.AddDays(365)) throw new ArgumentException("Activity date cannot be more than a year in the future.", nameof(date));

            groupActivity.Name = name;
            groupActivity.Description = description;
            groupActivity.Date = date;
            groupActivity.Group = group;
            groupActivity.Points = points;
            groupActivity.Status = status;

            await _unitOfWork.GroupActivities.UpdateAsync(groupActivity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task AddStudentToGroupActivityAsync(int studentId, int groupActivityId, DateTime participationDate)
        {
            var student = await GetStudentByIdAsync(studentId);
            var groupActivity = await _unitOfWork.GroupActivities.GetByIdAsync(groupActivityId);
            if (groupActivity == null) throw new ArgumentException("Group activity not found.", nameof(groupActivityId));
            if (participationDate > DateTime.UtcNow) throw new ArgumentException("Participation date cannot be in the future.", nameof(participationDate));
            if (student.Group != groupActivity.Group) throw new InvalidOperationException("Student is not in the same group as the activity.");

            var studentGroupActivity = new StudentGroupActivity
            {
                StudentId = studentId,
                GroupActivityId = groupActivityId,
                ParticipationDate = participationDate
            };

            await _unitOfWork.StudentGroupActivities.AddAsync(studentGroupActivity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<GroupActivity>> GetGroupActivitiesAsync(string? group = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _unitOfWork.GroupActivities.GetByGroupAsync(group, startDate, endDate);
        }

        public async Task<IEnumerable<StudentGroupActivity>> GetStudentGroupActivitiesAsync(int studentId)
        {
            await GetStudentByIdAsync(studentId);
            var studentGroupActivities = await _unitOfWork.StudentGroupActivities.GetAllAsync();
            return studentGroupActivities.Where(sga => sga.StudentId == studentId);
        }
    }
}