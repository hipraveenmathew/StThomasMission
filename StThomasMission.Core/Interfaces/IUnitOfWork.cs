using StThomasMission.Core.Entities;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAnnouncementRepository Announcements { get; }
        IAssessmentRepository Assessments { get; }
        IAttendanceRepository Attendances { get; }
        IAuditLogRepository AuditLogs { get; }
        IFamilyRepository Families { get; }
        IFamilyMemberRepository FamilyMembers { get; }
        IGradeRepository Grades { get; }
        IGroupRepository Groups { get; }
        IGroupActivityRepository GroupActivities { get; }
        IMassTimingRepository MassTimings { get; }
        IMessageLogRepository MessageLogs { get; }
        IMigrationLogRepository MigrationLogs { get; }
        IStudentRepository Students { get; }
        IStudentAcademicRecordRepository StudentAcademicRecords { get; }
        IStudentGroupActivityRepository StudentGroupActivities { get; }
        ITeacherAssignmentRepository TeacherAssignments { get; }
        IWardRepository Wards { get; }
        ICountStorageRepository CountStorage { get; }


        // Using generic repository for entities without custom methods
        IRepository<AssessmentSummary> AssessmentSummaries { get; }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> CompleteAsync();
    }
}