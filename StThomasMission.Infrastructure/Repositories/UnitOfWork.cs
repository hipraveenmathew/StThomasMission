using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Repositories; // <-- THIS IS THE CRITICAL LINE TO ADD/VERIFY

namespace StThomasMission.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StThomasMissionDbContext _context;

        // Private backing fields for lazy loading
        private IAnnouncementRepository? _announcements;
        private IAssessmentRepository? _assessments;
        private IAttendanceRepository? _attendances;
        private IAuditLogRepository? _auditLogs;
        private IFamilyRepository? _families;
        private IFamilyMemberRepository? _familyMembers;
        private IGradeRepository? _grades;
        private IGroupRepository? _groups;
        private IGroupActivityRepository? _groupActivities;
        private IMassTimingRepository? _massTimings;
        private IMessageLogRepository? _messageLogs;
        private IMigrationLogRepository? _migrationLogs;
        private IStudentRepository? _students;
        private IStudentAcademicRecordRepository? _studentAcademicRecords;
        private IStudentGroupActivityRepository? _studentGroupActivities;
        private ITeacherAssignmentRepository? _teacherAssignments;
        private IWardRepository? _wards;
        private IRepository<AssessmentSummary>? _assessmentSummaries;
        private ICountStorageRepository? _countStorage;

        public UnitOfWork(StThomasMissionDbContext context)
        {
            _context = context;
        }

        // Lazy-loaded repository properties
        public IAnnouncementRepository Announcements => _announcements ??= new AnnouncementRepository(_context);
        public IAssessmentRepository Assessments => _assessments ??= new AssessmentRepository(_context);
        public IAttendanceRepository Attendances => _attendances ??= new AttendanceRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
        public IFamilyRepository Families => _families ??= new FamilyRepository(_context);
        public IFamilyMemberRepository FamilyMembers => _familyMembers ??= new FamilyMemberRepository(_context);
        public IGradeRepository Grades => _grades ??= new GradeRepository(_context);
        public IGroupRepository Groups => _groups ??= new GroupRepository(_context);
        public IGroupActivityRepository GroupActivities => _groupActivities ??= new GroupActivityRepository(_context);
        public IMassTimingRepository MassTimings => _massTimings ??= new MassTimingRepository(_context);
        public IMessageLogRepository MessageLogs => _messageLogs ??= new MessageLogRepository(_context);
        public IMigrationLogRepository MigrationLogs => _migrationLogs ??= new MigrationLogRepository(_context);
        public IStudentRepository Students => _students ??= new StudentRepository(_context);
        public IStudentAcademicRecordRepository StudentAcademicRecords => _studentAcademicRecords ??= new StudentAcademicRecordRepository(_context);
        public IStudentGroupActivityRepository StudentGroupActivities => _studentGroupActivities ??= new StudentGroupActivityRepository(_context);
        public ITeacherAssignmentRepository TeacherAssignments => _teacherAssignments ??= new TeacherAssignmentRepository(_context);
        public IWardRepository Wards => _wards ??= new WardRepository(_context);
        public IRepository<AssessmentSummary> AssessmentSummaries => _assessmentSummaries ??= new Repository<AssessmentSummary>(_context);
        public ICountStorageRepository CountStorage => _countStorage ??= new CountStorageRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}