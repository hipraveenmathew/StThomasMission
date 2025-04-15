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
        [HttpGet]
        public IActionResult SendFeeReminder()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendFeeReminder(int studentId, string feeDetails)
        {
            await _communicationService.SendFeeReminderAsync(studentId, feeDetails);
            TempData["Success"] = "Fee reminder sent successfully!";
            return RedirectToAction("SendFeeReminder");
        }

        [HttpGet]
        public IActionResult SendGroupUpdate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendGroupUpdate(SendGroupUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _communicationService.SendGroupUpdateAsync(model.GroupName, model.UpdateMessage, model.CommunicationMethods);
            TempData["Success"] = "Group update sent successfully!";
            return RedirectToAction("SendGroupUpdate");
        }
    }
}