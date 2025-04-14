using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student> AddAsync(Student student);
        Task<Student?> GetByIdAsync(int id);
        Task UpdateAsync(Student student);
        Task DeleteAsync(int id);
        Task<IEnumerable<Student>> GetByGradeAsync(string grade);
        Task<IEnumerable<Student>> GetByFamilyIdAsync(int familyId);
        Task<IEnumerable<Student>> GetAllAsync();
    }
}