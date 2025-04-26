using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Families.Models;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin,ParishPriest")]
    public class AnnouncementsController : Controller
    {
        private readonly ICommunicationService _communicationService;

        public AnnouncementsController(ICommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SendAnnouncementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendAnnouncementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                if (!model.CommunicationMethods.Any())
                {
                    ModelState.AddModelError("CommunicationMethods", "Please select at least one communication method.");
                    return View("Index", model);
                }

                foreach (var method in model.CommunicationMethods)
                {
                    await _communicationService.SendAnnouncementAsync(model.Message, model.Ward, method);
                }

                model.SuccessMessage = "Announcement sent successfully!";
                model.Message = string.Empty; // Clear the message after sending
                model.CommunicationMethods = new List<string>(); // Clear selected methods
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to send announcement: {ex.Message}");
            }

            return View("Index", model);
        }
    }
}