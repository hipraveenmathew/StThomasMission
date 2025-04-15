using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;

namespace StThomasMission.Services.Services
{
    public class CatechismService : ICatechismService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public CatechismService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task PromoteStudentsAsync(string grade, int academicYear)
        {
            var students = await _unitOfWork.Students.GetByGradeAsync(grade);
            students = students.Where(s => s.AcademicYear == academicYear && s.Status == "Active").ToList();

            if (!students.Any()) return;

            var gradeNumber = int.Parse(grade.Replace("Year ", ""));
            foreach (var student in students)
            {
                if (gradeNumber >= 12)
                {
                    student.Status = "Graduated";
                }
                else
                {
                    student.Grade = $"Year {gradeNumber + 1}";
                }
                await _unitOfWork.Students.UpdateAsync(student);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(
                "system",
                "PromoteStudents",
                "Student",
                0, // No single entity ID; can log individual student IDs if needed
                $"Promoted students in {grade} for academic year {academicYear}"
            );
        }

        public async Task RecordStudentGroupActivityAsync(int studentId, int groupActivityId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            var groupActivity = await _unitOfWork.GroupActivities.GetByIdAsync(groupActivityId);

            if (student == null || groupActivity == null)
            {
                throw new Exception("Student or Group Activity not found");
            }

            var studentGroupActivity = new StudentGroupActivity
            {
                StudentId = studentId,
                GroupActivityId = groupActivityId,
                ParticipationDate = DateTime.UtcNow
            };

            _unitOfWork._context.StudentGroupActivities.Add(studentGroupActivity);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(
                "system",
                "RecordGroupActivity",
                "Student",
                studentId,
                $"Recorded group activity participation for student {studentId}, activity {groupActivityId}"
            );
        }

        public async Task<Student> AddStudentAsync(int familyMemberId, string grade, int academicYear, string group)
        {
            var student = new Student
            {
                FamilyMemberId = familyMemberId,
                Grade = grade,
                AcademicYear = academicYear,
                Group = group,
                Status = "Active"
            };
            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();
            return student;
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _unitOfWork.Students.GetByIdAsync(studentId);
        }

        public async Task UpdateStudentAsync(Student student)
        {
            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkPassFailAsync(int studentId, bool passed)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new Exception("Student not found");

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
            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();
        }

       

        public async Task MarkAttendanceAsync(int studentId, DateTime date, string description, bool isPresent)
        {
            var attendance = new Attendance
            {
                StudentId = studentId,
                Date = date,
                Description = description,
                IsPresent = isPresent
            };
            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.CompleteAsync();
        }

        public async Task AddAssessmentAsync(int studentId, string name, int marks, int totalMarks, bool isMajor)
        {
            var assessment = new Assessment
            {
                StudentId = studentId,
                Name = name,
                Marks = marks,
                TotalMarks = totalMarks,
                Date = DateTime.UtcNow,
                IsMajor = isMajor
            };
            await _unitOfWork.Assessments.AddAsync(assessment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task AddGroupActivityAsync(string groupName, string activityName, int points)
        {
            var groupActivity = new GroupActivity
            {
                Group = groupName, // Changed from GroupName to Group
                Name = activityName, // Changed from ActivityName to Name
                Points = points,
                Date = DateTime.UtcNow,
                Status = "Active" // Added to match GroupActivity entity
            };
            await _unitOfWork.GroupActivities.AddAsync(groupActivity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade)
        {
            return await _unitOfWork.Students.GetByGradeAsync(grade);
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId)
        {
            return await _unitOfWork.Attendances.GetByStudentIdAsync(studentId);
        }

        public async Task<IEnumerable<Assessment>> GetAssessmentsByStudentAsync(int studentId)
        {
            return await _unitOfWork.Assessments.GetByStudentIdAsync(studentId);
        }

        public async Task<IEnumerable<GroupActivity>> GetGroupActivitiesAsync(string groupName)
        {
            return await _unitOfWork.GroupActivities.GetByGroupAsync(groupName);
        }
        //public async Task RecordStudentGroupActivityAsync(int studentId, int groupActivityId)
        //{
        //    var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        //    var groupActivity = await _unitOfWork.GroupActivities.GetByIdAsync(groupActivityId);

        //    if (student == null || groupActivity == null)
        //    {
        //        throw new Exception("Student or Group Activity not found");
        //    }

        //    var studentGroupActivity = new StudentGroupActivity
        //    {
        //        StudentId = studentId,
        //        GroupActivityId = groupActivityId,
        //        ParticipationDate = DateTime.UtcNow
        //    };

        //    // Assuming a DbSet for StudentGroupActivity exists in the DbContext
        //    _unitOfWork._context.StudentGroupActivities.Add(studentGroupActivity);
        //    await _unitOfWork.CompleteAsync();
        //}
    }
}