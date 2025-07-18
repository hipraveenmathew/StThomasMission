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
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.ParishPriest}")]
    public class MassTimingsController : Controller
    {
        private readonly IMassTimingService _massTimingService;
        private readonly ILogger<MassTimingsController> _logger;

        public MassTimingsController(IMassTimingService massTimingService, ILogger<MassTimingsController> logger)
        {
            _massTimingService = massTimingService;
            _logger = logger;
        }

        // GET: /Admin/MassTimings
        public async Task<IActionResult> Index()
        {
            var timings = await _massTimingService.GetCurrentAndUpcomingMassesAsync();
            var model = new MassTimingIndexViewModel
            {
                MassTimings = timings.ToList()
            };
            return View(model);
        }

        // GET: /Admin/MassTimings/Create
        public IActionResult Create()
        {
            var model = new MassTimingFormViewModel();
            return View(model);
        }

        // POST: /Admin/MassTimings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MassTimingFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new CreateMassTimingRequest
                {
                    Day = model.Day,
                    Time = model.Time,
                    Location = model.Location,
                    Type = model.Type,
                    WeekStartDate = model.WeekStartDate
                };
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _massTimingService.AddMassTimingAsync(request, userId);

                TempData["Success"] = "Mass timing created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating mass timing.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the mass timing.");
                return View(model);
            }
        }

        // GET: /Admin/MassTimings/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var dto = await _massTimingService.GetMassTimingByIdAsync(id);
                var model = new MassTimingFormViewModel
                {
                    Id = dto.Id,
                    Day = dto.Day,
                    Time = dto.Time,
                    Location = dto.Location,
                    Type = dto.Type,
                    WeekStartDate = dto.WeekStartDate
                };
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Admin/MassTimings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MassTimingFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new UpdateMassTimingRequest
                {
                    Day = model.Day,
                    Time = model.Time,
                    Location = model.Location,
                    Type = model.Type,
                    WeekStartDate = model.WeekStartDate
                };
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _massTimingService.UpdateMassTimingAsync(id, request, userId);

                TempData["Success"] = "Mass timing updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mass timing {MassTimingId}", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the mass timing.");
                return View(model);
            }
        }

        // GET: /Admin/MassTimings/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var timing = await _massTimingService.GetMassTimingByIdAsync(id);
                return View(timing);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Admin/MassTimings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _massTimingService.DeleteMassTimingAsync(id, userId);
                TempData["Success"] = "Mass timing deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting mass timing {MassTimingId}", id);
                TempData["Error"] = "An error occurred while deleting the mass timing.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}