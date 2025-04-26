using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<Student>> GetByGroupIdAsync(int groupId);
        Task<IEnumerable<Student>> GetByStatusAsync(StudentStatus status);
    }
}