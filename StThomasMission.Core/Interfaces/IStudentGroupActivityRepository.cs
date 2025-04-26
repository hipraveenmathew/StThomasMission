using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentGroupActivityRepository : IRepository<StudentGroupActivity>
    {
        Task<IEnumerable<StudentGroupActivity>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<StudentGroupActivity>> GetByGroupActivityIdAsync(int groupActivityId);
    }
}