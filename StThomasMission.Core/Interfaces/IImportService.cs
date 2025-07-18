using StThomasMission.Core.DTOs;
using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IImportService
    {
        Task<ImportResultDto> ImportDataAsync(Stream fileStream, string userId);
    }
}