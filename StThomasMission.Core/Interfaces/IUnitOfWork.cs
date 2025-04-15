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
        // Repositories
        IStudentRepository Students { get; }
        IRepository<Family> Families { get; }
        IRepository<FamilyMember> FamilyMembers { get; }
        IRepository<Attendance> Attendances { get; }
        IRepository<Assessment> Assessments { get; }
        IRepository<GroupActivity> GroupActivities { get; }
        IRepository<StudentGroupActivity> StudentGroupActivities { get; }
        IRepository<MessageLog> MessageLogs { get; }
        IRepository<AuditLog> AuditLogs { get; }

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CompleteAsync();
        
    }
}
