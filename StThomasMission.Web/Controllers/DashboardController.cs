using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommunicationService _communicationService;

        public DashboardController(IUnitOfWork unitOfWork, ICommunicationService communicationService)
        {
            _unitOfWork = unitOfWork;
            _communicationService = communicationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var familyMember = await _unitOfWork.FamilyMembers.GetByUserIdAsync(userId);

            string? userWard = null;
            if (familyMember != null)
            {
                var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);
                userWard = family?.Ward;
            }

            var messages = (await _unitOfWork.MessageLogs.GetAllAsync())
                .OrderByDescending(m => m.SentAt)
                .Take(10)
                .ToList();

            if (!string.IsNullOrEmpty(userWard))
            {
                messages = messages
                    .Where(m => m.Message.Contains(userWard) || !m.Message.Contains("Ward"))
                    .ToList();
            }

            var upcomingEvents = (await _unitOfWork.GroupActivities.GetAllAsync())
                .Where(e => e.Date >= DateTime.Today && e.Date <= DateTime.Today.AddDays(30))
                .OrderBy(e => e.Date)
                .Take(5)
                .ToList();

            var students = new List<Student>();

            if (User.IsInRole("Teacher") || User.IsInRole("HeadTeacher") || User.IsInRole("ParishPriest"))
            {
                students = (await _unitOfWork.Students.GetAllAsync())
                    .OrderByDescending(s => s.AcademicYear)
                    .Take(5)
                    .ToList();
            }
            else if (User.IsInRole("Parent") && familyMember != null)
            {
                students = (await _unitOfWork.Students.GetByFamilyIdAsync(familyMember.FamilyId))
                    .ToList();
            }

            var model = new DashboardViewModel
            {
                RecentAnnouncements = messages,
                UpcomingEvents = upcomingEvents,
                StudentProgress = students
            };

            return View(model);
        }
    }
}
