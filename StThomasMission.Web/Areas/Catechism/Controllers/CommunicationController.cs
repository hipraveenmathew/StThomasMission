using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Catechism.Models;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "Teacher,HeadTeacher,ParishAdmin,ParishPriest")]
    public class CommunicationController : Controller
    {
        private readonly ICommunicationService _communicationService;

        public CommunicationController(ICommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [Authorize(Roles = "Teacher,HeadTeacher")]
        [HttpGet]
        public IActionResult SendAbsenteeNotifications()
        {
            return View(new AbsenteeNotificationViewModel());
        }

        [Authorize(Roles = "Teacher,HeadTeacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAbsenteeNotifications(AbsenteeNotificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                foreach (var method in model.CommunicationMethods)
                {
                    await _communicationService.SendAbsenteeNotificationsAsync(model.Grade, method);
                }
                model.SuccessMessage = "Absentee notifications sent successfully!";
                model.Grade = string.Empty;
                model.CommunicationMethods = new List<string>();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to send notifications: {ex.Message}");
            }

            return View(model);
        }

        [Authorize(Roles = "ParishAdmin,ParishPriest")]
        [HttpGet]
        public IActionResult SendAnnouncement()
        {
            return View(new AnnouncementViewModel());
        }

        [Authorize(Roles = "ParishAdmin,ParishPriest")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAnnouncement(AnnouncementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (!model.CommunicationMethods.Any())
                {
                    ModelState.AddModelError("CommunicationMethods", "Please select at least one communication method.");
                    return View(model);
                }

                foreach (var method in model.CommunicationMethods)
                {
                    await _communicationService.SendAnnouncementAsync(model.Message, model.Ward, method);
                }
                model.SuccessMessage = "Announcement sent successfully!";
                model.Message = string.Empty;
                model.Ward = 0;
                model.CommunicationMethods = new List<string>();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to send announcement: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult SendFeeReminder()
        {
            return View(new FeeReminderViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFeeReminder(FeeReminderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _communicationService.SendFeeReminderAsync(model.FamilyId, model.FeeDetails);
                model.SuccessMessage = "Fee reminder sent successfully!";
                model.FamilyId = 0;
                model.FeeDetails = string.Empty;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to send fee reminder: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult SendGroupUpdate()
        {
            return View(new SendGroupUpdateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendGroupUpdate(SendGroupUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (!model.CommunicationMethods.Any())
                {
                    ModelState.AddModelError("CommunicationMethods", "Please select at least one communication method.");
                    return View(model);
                }

                foreach (var method in model.CommunicationMethods)
                {
                    await _communicationService.SendGroupUpdateAsync(model.GroupId, model.UpdateMessage, method);
                }
                model.SuccessMessage = "Group update sent successfully!";
                model.GroupId = 0;
                model.UpdateMessage = string.Empty;
                model.CommunicationMethods = new List<string>();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to send group update: {ex.Message}");
            }

            return View(model);
        }
    }
}