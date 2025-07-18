using StThomasMission.Core.Enums;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> ExportStudentsByGradeAsync(int gradeId, ReportFormat format);
        Task<byte[]> ExportFamiliesByWardAsync(int wardId, ReportFormat format);
    }
}