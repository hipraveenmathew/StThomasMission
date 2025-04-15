using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IBackupService _backupService;

        public AdminController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                string backupPath = await _backupService.CreateBackupAsync();
                TempData["Success"] = $"Backup created successfully at {backupPath}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create backup: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBackup(string backupFileName)
        {
            var backupStream = await _backupService.GetBackupFileAsync(backupFileName);
            if (backupStream == null)
            {
                return NotFound();
            }

            return File(backupStream, "application/zip", backupFileName);
        }
    }
}