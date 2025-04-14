using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Catechism.Models;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "Teacher, HeadTeacher, ParishAdmin, ParishPriest")]
    public class CommunicationController : Controller
    {
        private readonly ICommunicationService _communicationService;

        public CommunicationController(ICommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [Authorize(Roles = "Teacher, HeadTeacher")]
        public IActionResult SendAbsenteeNotifications()
        {
            return View(new AbsenteeNotificationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher, HeadTeacher")]
        public async Task<IActionResult> SendAbsenteeNotifications(AbsenteeNotificationViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _communicationService.SendAbsenteeNotificationsAsync(model.Grade);
                TempData["Success"] = "Absentee notifications sent successfully!";
                return RedirectToAction(nameof(SendAbsenteeNotifications));
            }
            return View(model);
        }

        [Authorize(Roles = "ParishAdmin, ParishPriest")]
        public IActionResult SendAnnouncement()
        {
            return View(new AnnouncementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ParishAdmin, ParishPriest")]
        public async Task<IActionResult> SendAnnouncement(AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _communicationService.SendAnnouncementAsync(model.Message, model.Ward);
                TempData["Success"] = "Announcement sent successfully!";
                return RedirectToAction(nameof(SendAnnouncement));
            }
            return View(model);
        }
    }
}