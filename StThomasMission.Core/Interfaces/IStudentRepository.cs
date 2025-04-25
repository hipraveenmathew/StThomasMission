using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Repository for Student-specific operations.
    /// </summary>
    public interface IStudentRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<Student>> GetByGroupAsync(string group);
        Task<IEnumerable<Student>> GetByStatusAsync(StudentStatus status);
    }
}