using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync();
        Task<Stream> GetBackupFileAsync(string backupFileName);
    }
}