using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StThomasMission.Core.Constants;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using StThomasMission.Web.Areas.Church.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Church.Controllers
{
    [Area("Church")]
    [Authorize(Roles = $"{UserRoles.ParishAdmin},{UserRoles.ParishPriest}")]
    public class AnnouncementsController : Controller
    {
        private readonly ICommunicationService _communicationService;
        private readonly IWardService _wardService;
        private readonly ILogger<AnnouncementsController> _logger;

        public AnnouncementsController(
            ICommunicationService communicationService,
            IWardService wardService,
            ILogger<AnnouncementsController> logger)
        {
            _communicationService = communicationService;
            _wardService = wardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new SendAnnouncementViewModel
            {
                AvailableWards = await GetWardsSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SendAnnouncementViewModel model)
        {
            // Always repopulate the dropdown in case of failure
            model.AvailableWards = await GetWardsSelectListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new BroadcastRequest
                {
                    TargetId = model.WardId,
                    Message = model.Message,
                    Channel = model.Channel
                };

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _communicationService.SendAnnouncementToWardAsync(request, userId);

                TempData["Success"] = $"Announcement successfully sent to {model.AvailableWards.FirstOrDefault(w => w.Value == model.WardId.ToString())?.Text} via {model.Channel}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending announcement to ward {WardId}", model.WardId);
                ModelState.AddModelError(string.Empty, $"Failed to send announcement: {ex.Message}");
                return View(model);
            }
        }

        private async Task<SelectList> GetWardsSelectListAsync()
        {
            var wards = await _wardService.GetAllWardsAsync();
            return new SelectList(wards, "Id", "Name");
        }
    }
}