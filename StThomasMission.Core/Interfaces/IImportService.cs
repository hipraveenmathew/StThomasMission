using StThomasMission.Core.Enums;
using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IImportService
    {
        Task ImportFamiliesAndStudentsAsync(Stream fileStream, ImportType fileType);
        Task ImportWardsAsync(Stream fileStream, ImportType fileType);
    }
}