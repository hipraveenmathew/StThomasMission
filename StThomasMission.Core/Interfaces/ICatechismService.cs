using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface ICatechismService
    {
        Task<Student> AddStudentAsync(int familyMemberId, string grade, int academicYear, string group);
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task UpdateStudentAsync(Student student);
        Task MarkPassFailAsync(int studentId, bool passed);
        Task PromoteStudentsAsync(string grade, int academicYear);
        Task MarkAttendanceAsync(int studentId, DateTime date, string description, bool isPresent);
        Task AddAssessmentAsync(int studentId, string name, int marks, int totalMarks, bool isMajor);
        Task AddGroupActivityAsync(string groupName, string activityName, int points);
        Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade);
        Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId);
        Task<IEnumerable<Assessment>> GetAssessmentsByStudentAsync(int studentId);
        Task<IEnumerable<GroupActivity>> GetGroupActivitiesAsync(string groupName);
    }
}