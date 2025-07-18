using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Constants;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using StThomasMission.Web.Areas.Admin.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.ParishAdmin},{UserRoles.ParishPriest}")]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ILogger<AnnouncementsController> _logger;

        public AnnouncementsController(IAnnouncementService announcementService, ILogger<AnnouncementsController> logger)
        {
            _announcementService = announcementService;
            _logger = logger;
        }

        // GET: /Admin/Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementService.GetActiveAnnouncementsAsync();
            var model = new AnnouncementIndexViewModel
            {
                Announcements = announcements.ToList()
            };
            return View(model);
        }

        // GET: /Admin/Announcements/Create
        public IActionResult Create()
        {
            var model = new AnnouncementFormViewModel();
            return View(model);
        }

        // POST: /Admin/Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new CreateAnnouncementRequest
                {
                    Title = model.Title,
                    Description = model.Description,
                    PostedDate = model.PostedDate,
                    IsActive = model.IsActive
                };
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _announcementService.CreateAnnouncementAsync(request, userId);

                TempData["Success"] = "Announcement created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating announcement.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the announcement.");
                return View(model);
            }
        }

        // GET: /Admin/Announcements/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var announcementDto = await _announcementService.GetAnnouncementByIdAsync(id);
                var model = new AnnouncementFormViewModel
                {
                    Id = announcementDto.Id,
                    Title = announcementDto.Title,
                    Description = announcementDto.Description,
                    PostedDate = announcementDto.PostedDate,
                    IsActive = announcementDto.IsActive
                };
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Admin/Announcements/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new UpdateAnnouncementRequest
                {
                    Title = model.Title,
                    Description = model.Description,
                    PostedDate = model.PostedDate,
                    IsActive = model.IsActive
                };
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _announcementService.UpdateAnnouncementAsync(id, request, userId);

                TempData["Success"] = "Announcement updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating announcement {AnnouncementId}", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the announcement.");
                return View(model);
            }
        }

        // GET: /Admin/Announcements/Delete/1
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                return View(announcement);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Admin/Announcements/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _announcementService.DeleteAnnouncementAsync(id, userId);
                TempData["Success"] = "Announcement deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting announcement {AnnouncementId}", id);
                TempData["Error"] = "An error occurred while deleting the announcement.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}