using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// Service for handling backup operations.
    /// </summary>
    public interface IBackupService
    {
        Task<string> CreateBackupAsync();
        Task<Stream> GetBackupFileAsync(string backupFileName);
    }
}
