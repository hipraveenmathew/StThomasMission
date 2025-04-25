using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Families.Models;
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
        public IActionResult SendAnnouncement()
        {
            return View(new SendAnnouncementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAnnouncement(SendAnnouncementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            foreach (var method in model.CommunicationMethods)
            {
                await _communicationService.SendAnnouncementAsync(model.Message, model.Ward, method);
            }

            TempData["Success"] = "Announcement sent successfully!";
            return RedirectToAction("SendAnnouncement");
        }
    }
}
