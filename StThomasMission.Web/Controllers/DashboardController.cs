using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Linq;
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
            // Recent Announcements (last 5 messages sent to all or user's ward)
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var familyMember = await _unitOfWork.FamilyMembers.GetByUserIdAsync(userId);
            string userWard = familyMember != null ? (await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId))?.Ward : null;

            var messages = await _unitOfWork._context.MessageLogs
                .Where(m => m.Message.StartsWith("Dear") || m.Message.StartsWith("Announcement"))
                .OrderByDescending(m => m.SentAt)
                .Take(5)
                .ToListAsync();

            if (!string.IsNullOrEmpty(userWard))
            {
                messages = messages.Where(m => m.Message.Contains(userWard) || !m.Message.Contains("Ward")).ToList();
            }

            // Upcoming Events (Group Activities within the next 30 days)
            var upcomingEvents = await _unitOfWork.GroupActivities.GetAllAsync();
            upcomingEvents = upcomingEvents
                .Where(e => e.Date >= DateTime.Today && e.Date <= DateTime.Today.AddDays(30))
                .OrderBy(e => e.Date)
                .Take(5)
                .ToList();

            // Student Progress (for Teachers/HeadTeachers/Parents)
            List<Student> students = new();
            if (User.IsInRole("Teacher") || User.IsInRole("HeadTeacher") || User.IsInRole("ParishPriest"))
            {
                students = (await _unitOfWork.Students.GetAllAsync()).Take(5).ToList();
            }
            else if (User.IsInRole("Parent") && familyMember != null)
            {
                students = (await _unitOfWork.Students.GetByFamilyIdAsync(familyMember.FamilyId)).ToList();
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

    public class DashboardViewModel
    {
        public List<MessageLog> RecentAnnouncements { get; set; }
        public List<GroupActivity> UpcomingEvents { get; set; }
        public List<Student> StudentProgress { get; set; }
    }
}