using Microsoft.EntityFrameworkCore.Storage;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Repositories;

namespace StThomasMission.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StThomasMissionDbContext _dbContext;

        public UnitOfWork(StThomasMissionDbContext dbContext)
        {
            _dbContext = dbContext;
            Students = new StudentRepository(_dbContext);
            Families = new Repository<Family>(_dbContext);
            FamilyMembers = new Repository<FamilyMember>(_dbContext);
            Attendances = new Repository<Attendance>(_dbContext);
            Assessments = new Repository<Assessment>(_dbContext);
            GroupActivities = new Repository<GroupActivity>(_dbContext);
            StudentGroupActivities = new Repository<StudentGroupActivity>(_dbContext);
            MessageLogs = new Repository<MessageLog>(_dbContext);
            AuditLogs = new Repository<AuditLog>(_dbContext);
        }

        public IStudentRepository Students { get; }
        public IRepository<Family> Families { get; }
        public IRepository<FamilyMember> FamilyMembers { get; }
        public IRepository<Attendance> Attendances { get; }
        public IRepository<Assessment> Assessments { get; }
        public IRepository<GroupActivity> GroupActivities { get; }
        public IRepository<StudentGroupActivity> StudentGroupActivities { get; }
        public IRepository<MessageLog> MessageLogs { get; }
        public IRepository<AuditLog> AuditLogs { get; }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
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
