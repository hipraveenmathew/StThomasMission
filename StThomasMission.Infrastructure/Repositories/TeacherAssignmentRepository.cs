using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;

namespace StThomasMission.Infrastructure.Repositories
{
    public class TeacherAssignmentRepository : Repository<TeacherAssignment>, ITeacherAssignmentRepository
    {
        public TeacherAssignmentRepository(StThomasMissionDbContext context) : base(context) { }

        // You can add custom methods for TeacherAssignments here if needed
    }
}