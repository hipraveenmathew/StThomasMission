using Microsoft.Extensions.Configuration;
using StThomasMission.Core.Interfaces;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _backupDirectory;
        private readonly string _databaseBackupPath;

        public BackupService(IConfiguration configuration)
        {
            _backupDirectory = configuration["BackupSettings:BackupDirectory"] ?? "Backups";
            _databaseBackupPath = configuration["BackupSettings:DatabaseBackupPath"] ?? Path.Combine(_backupDirectory, "Database");

            Directory.CreateDirectory(_backupDirectory);
            Directory.CreateDirectory(_databaseBackupPath);
        }

        public async Task<string> CreateBackupAsync()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"Backup_{timestamp}.zip";
            string backupFilePath = Path.Combine(_backupDirectory, backupFileName);
            string dbBackupFile = Path.Combine(_databaseBackupPath, $"db_backup_{timestamp}.sql");

            // Simulate database backup (replace with actual backup command for your DB)
            await File.WriteAllTextAsync(dbBackupFile, "/* Simulated SQL Server backup content */");

            using var zipStream = new FileStream(backupFilePath, FileMode.Create);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);
            var dbEntry = archive.CreateEntry(Path.GetFileName(dbBackupFile));
            using (var entryStream = dbEntry.Open())
            using (var fileStream = new FileStream(dbBackupFile, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(entryStream);
            }

            File.Delete(dbBackupFile);
            return backupFilePath;
        }

        public async Task<Stream> GetBackupFileAsync(string backupFileName)
        {
            string backupFilePath = Path.Combine(_backupDirectory, backupFileName);
            if (!File.Exists(backupFilePath))
                throw new FileNotFoundException("Backup file not found.", backupFileName);

            return new FileStream(backupFilePath, FileMode.Open, FileAccess.Read);
        }
    }
}