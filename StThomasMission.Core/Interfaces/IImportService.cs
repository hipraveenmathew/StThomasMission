using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IImportService
    {
        Task ImportFamiliesAndStudentsAsync(Stream fileStream, string fileType); // Supports Excel/CSV
    }
}