using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;

namespace StThomasMission.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StThomasMissionDbContext _context;
        public IFamilyRepository Families { get; private set; }
        public IStudentRepository Students { get; private set; }
        public IAttendanceRepository Attendances { get; private set; }
        public IAssessmentRepository Assessments { get; private set; }
        public IGroupActivityRepository GroupActivities { get; private set; } // Add this
        private IFamilyMemberRepository _familyMembers;

        public UnitOfWork(StThomasMissionDbContext context)
        {
            _context = context;
            Families = new FamilyRepository(_context);
            Students = new StudentRepository(_context);
            Attendances = new AttendanceRepository(_context);
            Assessments = new AssessmentRepository(_context);
            GroupActivities = new GroupActivityRepository(_context); // Add this
        }

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