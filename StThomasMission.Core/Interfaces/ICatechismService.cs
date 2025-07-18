using StThomasMission.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface ICatechismService
    {
        Task<StudentDetailDto> EnrollStudentAsync(EnrollStudentRequest request, string userId);

        Task UpdateStudentDetailsAsync(int studentId, UpdateStudentRequest request, string userId);

        Task MarkStudentsAsPassedOrFailAsync(int gradeId, IEnumerable<StudentPassFailRequest> results, string userId);

        Task PromoteStudentsInGradeAsync(int gradeId, string userId);
    }
}