using Microsoft.EntityFrameworkCore.Storage;
using StThomasMission.Core.Entities;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // --- ADDED Repositories ---
        IGradeRepository Grades { get; }
        ITeacherAssignmentRepository TeacherAssignments { get; }

        IStudentRepository Students { get; }
        IFamilyRepository Families { get; }
        IFamilyMemberRepository FamilyMembers { get; }
        IAttendanceRepository Attendances { get; }
        IAssessmentRepository Assessments { get; }
        IGroupRepository Groups { get; }
        IGroupActivityRepository GroupActivities { get; }
        IStudentGroupActivityRepository StudentGroupActivities { get; }
        IStudentAcademicRecordRepository StudentAcademicRecords { get; }
        IMessageLogRepository MessageLogs { get; }
        IAuditLogRepository AuditLogs { get; }
        IWardRepository Wards { get; }
        IMassTimingRepository MassTimings { get; }
        IAnnouncementRepository Announcements { get; }
        IMigrationLogRepository MigrationLogs { get; }
        IRepository<AssessmentSummary> AssessmentSummaries { get; }

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> CompleteAsync();
    }
}