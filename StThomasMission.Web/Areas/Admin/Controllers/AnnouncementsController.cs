using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,ParishAdmin,ParishPriest")]
    public class AnnouncementsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public AnnouncementsController(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        // GET: /Admin/Announcements
        public async Task<IActionResult> Index()
        {
            var announcements = await _unitOfWork.Announcements.GetAsync(a => !a.IsDeleted);
            return View(announcements);
        }

        // GET: /Admin/Announcements/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.CreatedBy = User.Identity.Name ?? "System";
                announcement.CreatedAt = DateTime.UtcNow;
                announcement.PostedDate = DateTime.Today;

                await _unitOfWork.Announcements.AddAsync(announcement);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync(User.Identity.Name, "Create", nameof(Announcement), announcement.Id.ToString(), $"Added announcement: {announcement.Title}");
                TempData["Success"] = "Announcement created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // GET: /Admin/Announcements/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: /Admin/Announcements/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement announcement)
        {
            if (id != announcement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingAnnouncement = await _unitOfWork.Announcements.GetByIdAsync(id);
                if (existingAnnouncement == null)
                {
                    return NotFound();
                }

                existingAnnouncement.Title = announcement.Title;
                existingAnnouncement.Description = announcement.Description;
                existingAnnouncement.IsActive = announcement.IsActive;
                existingAnnouncement.PostedDate = announcement.PostedDate;
                existingAnnouncement.UpdatedBy = User.Identity.Name ?? "System";

                await _unitOfWork.Announcements.UpdateAsync(existingAnnouncement);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync(User.Identity.Name, "Update", nameof(Announcement), announcement.Id.ToString(), $"Updated announcement: {announcement.Title}");
                TempData["Success"] = "Announcement updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // GET: /Admin/Announcements/Delete/1
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: /Admin/Announcements/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _unitOfWork.Announcements.GetByIdAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            announcement.IsDeleted = true;
            announcement.UpdatedBy = User.Identity.Name ?? "System";

            await _unitOfWork.Announcements.UpdateAsync(announcement);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(User.Identity.Name, "Delete", nameof(Announcement), announcement.Id.ToString(), $"Deleted announcement: {announcement.Title}");
            TempData["Success"] = "Announcement deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}