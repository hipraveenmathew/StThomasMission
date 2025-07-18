using StThomasMission.Core.DTOs;
using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IFamilyImportService
    {
        Task<ImportResultDto> ImportFamiliesFromExcelAsync(Stream fileStream, string userId);
    }
}