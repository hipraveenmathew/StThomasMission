using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;

        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // List available backups (simulated; in a real app, you might list files in the backup directory)
            var backups = Directory.GetFiles("Backups", "*.zip")
                .Select(f => new BackupViewModel
                {
                    FileName = Path.GetFileName(f),
                    CreatedAt = System.IO.File.GetCreationTime(f)
                })
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return View(backups);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            try
            {
                string backupPath = await _backupService.CreateBackupAsync();
                return Json(new { success = true, message = $"Backup created successfully at {Path.GetFileName(backupPath)}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to create backup: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("Backup file name is required.");
            }

            try
            {
                var backupStream = await _backupService.GetBackupFileAsync(fileName);
                if (backupStream == null)
                {
                    return NotFound("Backup file not found.");
                }

                return File(backupStream, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to download backup: {ex.Message}");
            }
        }
    }
}