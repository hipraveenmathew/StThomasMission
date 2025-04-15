using StThomasMission.Core.Entities;
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
        Task<IEnumerable<Student>> GetByGroupAsync(string group);
    }
}
