using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface ITeacherAssignmentRepository : IRepository<TeacherAssignment>
    {
        Task<IEnumerable<TeacherAssignmentDto>> GetAssignmentsByYearAsync(int academicYear);

        Task<TeacherAssignmentDto?> GetAssignmentForTeacherAsync(string userId, int academicYear);
    }
}