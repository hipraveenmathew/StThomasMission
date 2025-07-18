using StThomasMission.Core.DTOs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Core.Interfaces
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync();

        Task<Stream> GetBackupStreamAsync(string backupFileName);

        Task<IEnumerable<BackupFileDto>> GetBackupListAsync();
    }
}