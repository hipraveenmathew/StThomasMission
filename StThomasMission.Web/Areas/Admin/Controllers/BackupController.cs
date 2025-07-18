using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StThomasMission.Core.Constants;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Admin.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(IBackupService backupService, ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var backupDtos = await _backupService.GetBackupListAsync();

            var model = new BackupIndexViewModel
            {
                Backups = backupDtos.Select(b => new BackupViewModel
                {
                    FileName = b.FileName,
                    FileSizeInKB = (long)Math.Ceiling(b.FileSize / 1024.0),
                    CreatedAt = b.CreatedDate
                }).ToList()
            };

            return View(model);
        }

        // Only the Create action needs to be changed in your BackupController.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Backup creation initiated by user {User}.", User.Identity?.Name);
                string backupPath = await _backupService.CreateBackupAsync();
                TempData["Success"] = $"Backup '{Path.GetFileName(backupPath)}' created successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup creation failed.");
                TempData["Error"] = $"Failed to create backup: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("A file name must be provided.");
            }

            try
            {
                // Corrected: Changed GetBackupFileAsync to GetBackupStreamAsync
                var backupStream = await _backupService.GetBackupStreamAsync(fileName);
                _logger.LogInformation("User {User} downloaded backup file {FileName}.", User.Identity?.Name, fileName);

                return File(backupStream, "application/zip", fileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {User} attempted to download non-existent backup file {FileName}.", User.Identity?.Name, fileName);
                TempData["Error"] = $"Backup file not found: {fileName}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading backup file {FileName}.", fileName);
                TempData["Error"] = "An unexpected error occurred while downloading the backup.";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}