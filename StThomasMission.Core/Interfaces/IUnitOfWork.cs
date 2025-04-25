using Microsoft.EntityFrameworkCore.Storage;
using StThomasMission.Core.Entities;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Defines a Unit of Work for coordinating repository operations and transaction management.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        IFamilyRepository Families { get; }
        IFamilyMemberRepository FamilyMembers { get; }
        IAttendanceRepository Attendances { get; }
        IAssessmentRepository Assessments { get; }
        IGroupActivityRepository GroupActivities { get; }
        IStudentGroupActivityRepository StudentGroupActivities { get; }
        IRepository<MessageLog> MessageLogs { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IWardRepository Wards { get; }
        IRepository<User> Users { get; }

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task CompleteAsync();
    }
}