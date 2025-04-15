using StThomasMission.Core.Entities;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        IRepository<Family> Families { get; }
        IRepository<FamilyMember> FamilyMembers { get; }
        IRepository<Attendance> Attendances { get; }
        IRepository<Assessment> Assessments { get; }
        IRepository<GroupActivity> GroupActivities { get; }
        IRepository<StudentGroupActivity> StudentGroupActivities { get; }
        IRepository<MessageLog> MessageLogs { get; }
        IRepository<AuditLog> AuditLogs { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}