using StThomasMission.Core.Interfaces;

namespace StThomasMission.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IFamilyRepository Families { get; }
        IStudentRepository Students { get; }
        IAttendanceRepository Attendances { get; }
        IAssessmentRepository Assessments { get; }
        IGroupActivityRepository GroupActivities { get; }
        IFamilyMemberRepository FamilyMembers { get; }
        Task<int> CompleteAsync();
    }
}