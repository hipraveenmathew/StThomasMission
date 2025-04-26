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

        public UnitOfWork(StThomasMissionDbContext dbContext)
        {
            _dbContext = dbContext;
            Students = new StudentRepository(_dbContext);
            Families = new FamilyRepository(_dbContext);
            FamilyMembers = new FamilyMemberRepository(_dbContext);
            Attendances = new AttendanceRepository(_dbContext);
            Assessments = new AssessmentRepository(_dbContext);
            Groups = new GroupRepository(_dbContext);
            GroupActivities = new GroupActivityRepository(_dbContext);
            StudentGroupActivities = new StudentGroupActivityRepository(_dbContext);
            MessageLogs = new MessageLogRepository(_dbContext);
            AuditLogs = new AuditLogRepository(_dbContext);
            Wards = new WardRepository(_dbContext);
            MassTimings = new MassTimingRepository(_dbContext);
            Announcements = new AnnouncementRepository(_dbContext);
            MigrationLogs = new MigrationLogRepository(_dbContext);
            AssessmentSummaries = new Repository<AssessmentSummary>(_dbContext); // Added
        }

        public IStudentRepository Students { get; }
        public IFamilyRepository Families { get; }
        public IFamilyMemberRepository FamilyMembers { get; }
        public IAttendanceRepository Attendances { get; }
        public IAssessmentRepository Assessments { get; }
        public IGroupRepository Groups { get; }
        public IGroupActivityRepository GroupActivities { get; }
        public IStudentGroupActivityRepository StudentGroupActivities { get; }
        public IMessageLogRepository MessageLogs { get; }
        public IAuditLogRepository AuditLogs { get; }
        public IWardRepository Wards { get; }
        public IMassTimingRepository MassTimings { get; }
        public IAnnouncementRepository Announcements { get; }
        public IMigrationLogRepository MigrationLogs { get; }
        public IRepository<AssessmentSummary> AssessmentSummaries { get; } // Added

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