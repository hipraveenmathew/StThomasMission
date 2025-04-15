using Microsoft.Extensions.Configuration;
using StThomasMission.Core.Interfaces;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace StThomasMission.Core.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _backupDirectory;
        private readonly string _databaseBackupPath; // Path to DB backup tool or script

        public BackupService(IConfiguration configuration)
        {
            _backupDirectory = configuration["BackupSettings:BackupDirectory"] ?? "Backups";
            _databaseBackupPath = configuration["BackupSettings:DatabaseBackupPath"];
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        public async Task<string> CreateBackupAsync()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"Backup_{timestamp}.zip";
            string backupFilePath = Path.Combine(_backupDirectory, backupFileName);

            // Simulate database backup (e.g., using pg_dump for PostgreSQL or mysqldump for MySQL)
            string dbBackupFile = Path.Combine(_backupDirectory, $"db_backup_{timestamp}.sql");
            await File.WriteAllTextAsync(dbBackupFile, "Simulated database backup content");

            // Create a zip file containing the database backup
            using (var zipStream = new FileStream(backupFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var dbEntry = archive.CreateEntry(Path.GetFileName(dbBackupFile));
                using (var entryStream = dbEntry.Open())
                using (var fileStream = new FileStream(dbBackupFile, FileMode.Open))
                {
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            // Clean up temporary DB backup file
            File.Delete(dbBackupFile);

            return backupFilePath;
        }

        public async Task<Stream> GetBackupFileAsync(string backupFileName)
        {
            string backupFilePath = Path.Combine(_backupDirectory, backupFileName);
            if (!File.Exists(backupFilePath))
            {
                return null;
            }

            return new FileStream(backupFilePath, FileMode.Open, FileAccess.Read);
        }
    }
}