using Microsoft.EntityFrameworkCore.Storage;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StThomasMissionDbContext _dbContext;
        private readonly IRepository<User> _users;


        public UnitOfWork(StThomasMissionDbContext dbContext)
        {
            _dbContext = dbContext;
            Students = new StudentRepository(_dbContext);
            Families = new FamilyRepository(_dbContext);
            FamilyMembers = new FamilyMemberRepository(_dbContext);
            Attendances = new AttendanceRepository(_dbContext);
            Assessments = new AssessmentRepository(_dbContext);
            GroupActivities = new GroupActivityRepository(_dbContext);
            StudentGroupActivities = new StudentGroupActivityRepository(_dbContext);
            MessageLogs = new MessageLogRepository(_dbContext);
            AuditLogs = new AuditLogRepository(_dbContext);
            Wards = new WardRepository(_dbContext);
            _users = new UserRepository(_dbContext);
        }

        public IStudentRepository Students { get; }
        public IRepository<Family> Families { get; }
        public IFamilyMemberRepository FamilyMembers { get; }
        public IAttendanceRepository Attendances { get; }
        public IRepository<Assessment> Assessments { get; }
        public IGroupActivityRepository GroupActivities { get; }
        public IStudentGroupActivityRepository StudentGroupActivities { get; }
        public IRepository<MessageLog> MessageLogs { get; }
        public IRepository<AuditLog> AuditLogs { get; }
        public IRepository<Ward> Wards { get; }


        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _dbContext.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }

        public async Task CompleteAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}