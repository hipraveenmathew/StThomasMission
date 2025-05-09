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
    public class MassTimingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public MassTimingsController(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        // GET: /Admin/MassTimings
        public async Task<IActionResult> Index()
        {
            var massTimings = await _unitOfWork.MassTimings.GetAsync(mt => !mt.IsDeleted);
            return View(massTimings);
        }

        // GET: /Admin/MassTimings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/MassTimings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MassTiming massTiming)
        {
           
                massTiming.CreatedBy = User.Identity.Name ?? "System";
                massTiming.CreatedAt = DateTime.UtcNow;
                massTiming.WeekStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek); // Start of the week (Sunday)

                await _unitOfWork.MassTimings.AddAsync(massTiming);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync(User.Identity.Name, "Create", nameof(MassTiming), massTiming.Id.ToString(), $"Added mass timing: {massTiming.Day} at {massTiming.Time}");
                TempData["Success"] = "Mass timing created successfully.";
                return RedirectToAction(nameof(Index));
          
           // return View(massTiming);
        }

        // GET: /Admin/MassTimings/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var massTiming = await _unitOfWork.MassTimings.GetByIdAsync(id);
            if (massTiming == null)
            {
                return NotFound();
            }
            return View(massTiming);
        }

        // POST: /Admin/MassTimings/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MassTiming massTiming)
        {
            if (id != massTiming.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
                var existingTiming = await _unitOfWork.MassTimings.GetByIdAsync(id);
                if (existingTiming == null)
                {
                    return NotFound();
                }

                existingTiming.Day = massTiming.Day;
                existingTiming.Time = massTiming.Time;
                existingTiming.Location = massTiming.Location;
                existingTiming.Type = massTiming.Type;
                existingTiming.WeekStartDate = massTiming.WeekStartDate;
                existingTiming.UpdatedBy = User.Identity.Name ?? "System";

                await _unitOfWork.MassTimings.UpdateAsync(existingTiming);
                await _unitOfWork.CompleteAsync();

                await _auditService.LogActionAsync(User.Identity.Name, "Update", nameof(MassTiming), massTiming.Id.ToString(), $"Updated mass timing: {massTiming.Day} at {massTiming.Time}");
                TempData["Success"] = "Mass timing updated successfully.";
                return RedirectToAction(nameof(Index));
            }
        //    return View(massTiming);
        //}

        // GET: /Admin/MassTimings/Delete/1
        public async Task<IActionResult> Delete(int id)
        {
            var massTiming = await _unitOfWork.MassTimings.GetByIdAsync(id);
            if (massTiming == null)
            {
                return NotFound();
            }
            return View(massTiming);
        }

        // POST: /Admin/MassTimings/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var massTiming = await _unitOfWork.MassTimings.GetByIdAsync(id);
            if (massTiming == null)
            {
                return NotFound();
            }

            massTiming.IsDeleted = true;
            massTiming.UpdatedBy = User.Identity.Name ?? "System";
          

            await _unitOfWork.MassTimings.UpdateAsync(massTiming);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(User.Identity.Name, "Delete", nameof(MassTiming), massTiming.Id.ToString(), $"Deleted mass timing: {massTiming.Day} at {massTiming.Time}");
            TempData["Success"] = "Mass timing deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}