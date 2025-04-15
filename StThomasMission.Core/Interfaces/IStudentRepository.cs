using StThomasMission.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetByGroupAsync(string group);
    }
}