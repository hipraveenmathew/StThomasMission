using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentGroupActivityRepository : IRepository<StudentGroupActivity>
    {
        Task<IEnumerable<StudentGroupActivityDto>> GetByStudentIdAsync(int studentId);

        Task<IEnumerable<StudentGroupActivityDto>> GetByGroupActivityIdAsync(int groupActivityId);
        Task<List<int>> GetParticipantsByIdsAsync(int groupActivityId, List<int> studentIds);

    }
}