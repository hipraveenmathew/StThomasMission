using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service interface for handling data import operations.
    /// </summary>
    public interface IImportService
    {
        Task ImportFamiliesAndStudentsAsync(Stream fileStream, ImportType fileType);
        Task ImportWardsAsync(Stream fileStream, ImportType fileType);
    }

    public enum ImportType
    {
        Excel,
        CSV
    }
}