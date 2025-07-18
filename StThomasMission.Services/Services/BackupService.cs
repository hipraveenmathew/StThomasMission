using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _backupDirectory;
        private readonly StThomasMissionDbContext _context;
        private readonly ILogger<BackupService> _logger;

        public BackupService(IConfiguration configuration, StThomasMissionDbContext context, ILogger<BackupService> logger)
        {
            _context = context;
            _logger = logger;
            // Corrected way to read from configuration
            _backupDirectory = configuration["BackupSettings:DirectoryPath"] ?? "C:\\StThomasMission_Backups";
        }

        public async Task<string> CreateBackupAsync()
        {
            Directory.CreateDirectory(_backupDirectory);
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string tempDbBackupFile = Path.Combine(_backupDirectory, $"db_backup_{timestamp}.bak");
            string finalZipFile = Path.Combine(_backupDirectory, $"Backup_{timestamp}.zip");

            try
            {
                _logger.LogInformation("Starting database backup to temporary file: {TempBackupFile}", tempDbBackupFile);

                // Use the injected DbContext directly
                var connection = _context.Database.GetDbConnection();
                var dbName = connection.Database;

                string backupCommand = $"BACKUP DATABASE [{dbName}] TO DISK = N'{tempDbBackupFile}' WITH NOFORMAT, INIT, NAME = N'{dbName}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                await _context.Database.ExecuteSqlRawAsync(backupCommand);

                _logger.LogInformation("Database backup completed successfully. Zipping file...");

                using (var zipArchive = ZipFile.Open(finalZipFile, ZipArchiveMode.Create))
                {
                    zipArchive.CreateEntryFromFile(tempDbBackupFile, Path.GetFileName(tempDbBackupFile));
                }

                _logger.LogInformation("Backup successfully created at {ZipFile}", finalZipFile);
                return finalZipFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the backup process.");
                if (File.Exists(finalZipFile)) File.Delete(finalZipFile);
                throw;
            }
            finally
            {
                if (File.Exists(tempDbBackupFile))
                {
                    File.Delete(tempDbBackupFile);
                    _logger.LogInformation("Temporary backup file deleted.");
                }
            }
        }

        public Task<Stream> GetBackupStreamAsync(string backupFileName)
        {
            string backupFilePath = Path.Combine(_backupDirectory, backupFileName);
            if (!File.Exists(backupFilePath))
            {
                _logger.LogWarning("Attempted to access non-existent backup file: {BackupFileName}", backupFileName);
                throw new FileNotFoundException("Backup file not found.", backupFileName);
            }

            var fileStream = new FileStream(backupFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return Task.FromResult<Stream>(fileStream);
        }

        public Task<IEnumerable<BackupFileDto>> GetBackupListAsync()
        {
            var directoryInfo = new DirectoryInfo(_backupDirectory);
            if (!directoryInfo.Exists)
            {
                return Task.FromResult(Enumerable.Empty<BackupFileDto>());
            }

            var backupFiles = directoryInfo.GetFiles("*.zip")
                .OrderByDescending(f => f.CreationTimeUtc)
                .Select(f => new BackupFileDto
                {
                    FileName = f.Name,
                    FileSize = f.Length,
                    CreatedDate = f.CreationTimeUtc
                });

            return Task.FromResult<IEnumerable<BackupFileDto>>(backupFiles);
        }
    }
}