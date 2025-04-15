using StThomasMission.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing Catechism-related operations.
    /// </summary>
    public interface ICatechismService
    {
        Task<Student> GetStudentByIdAsync(int studentId);
        Task<IEnumerable<Student>> GetStudentsByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetStudentsByGroupAsync(string group);
        Task AddStudentAsync(int familyMemberId, int academicYear, string grade, string? group, string? studentOrganisation);
        Task UpdateStudentAsync(int studentId, string grade, string? group, string? studentOrganisation, string status, string? migratedTo);
        Task MarkPassFailAsync(int studentId, bool passed);
        Task PromoteStudentsAsync(string grade, int academicYear);

        Task AddAttendanceAsync(int studentId, DateTime date, string description, bool isPresent);
        Task UpdateAttendanceAsync(int attendanceId, bool isPresent, string description);
        Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Attendance>> GetAttendanceByGradeAsync(string grade, DateTime date);

        Task AddAssessmentAsync(int studentId, string name, int marks, int totalMarks, DateTime date, bool isMajor);
        Task UpdateAssessmentAsync(int assessmentId, string name, int marks, int totalMarks, DateTime date, bool isMajor);
        Task<IEnumerable<Assessment>> GetAssessmentsByStudentAsync(int studentId, bool? isMajor = null);
        Task<IEnumerable<Assessment>> GetAssessmentsByGradeAsync(string grade, DateTime? startDate = null, DateTime? endDate = null);

        Task AddGroupActivityAsync(string name, string description, DateTime date, string group, int points);
        Task UpdateGroupActivityAsync(int groupActivityId, string name, string description, DateTime date, string group, int points, string status);
        Task AddStudentToGroupActivityAsync(int studentId, int groupActivityId, DateTime participationDate);
        Task<IEnumerable<GroupActivity>> GetGroupActivitiesAsync(string? group = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<StudentGroupActivity>> GetStudentGroupActivitiesAsync(int studentId);
    }
}
