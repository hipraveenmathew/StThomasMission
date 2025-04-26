using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IFamilyService _familyService;
        private readonly IStudentService _studentService;
        private readonly IGroupActivityService _groupActivityService; // Changed from ICatechismService
        private readonly ICommunicationService _communicationService;

        public DashboardController(
            IFamilyService familyService,
            IStudentService studentService,
            IGroupActivityService groupActivityService,
            ICommunicationService communicationService)
        {
            _familyService = familyService;
            _studentService = studentService;
            _groupActivityService = groupActivityService; // Updated
            _communicationService = communicationService;
        }

        public async Task<IActionResult> Index(int messagePageNumber = 1, int eventPageNumber = 1, int studentPageNumber = 1, int pageSize = 5)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var familyMember = await _familyService.GetFamilyMemberByUserIdAsync(userId);

                int? userWardId = null;
                if (familyMember != null)
                {
                    var family = await _familyService.GetFamilyByIdAsync(familyMember.FamilyId);
                    userWardId = family?.WardId;
                }

                // Recent Announcements with Pagination
                var messagesQuery = _communicationService.GetMessageHistoryQueryable();
                if (userWardId.HasValue)
                {
                    messagesQuery = messagesQuery.Where(m =>
                        m.Message.Contains($"Ward {userWardId}") || !m.Message.Contains("Ward"));
                }
                messagesQuery = messagesQuery.OrderByDescending(m => m.SentAt);
                int messagesTotal = await messagesQuery.CountAsync();
                var recentAnnouncements = await messagesQuery
                    .Skip((messagePageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                var pagedAnnouncements = new PaginatedList<MessageLog>(recentAnnouncements, messagesTotal, messagePageNumber, pageSize);

                // Upcoming Events with Pagination
                var upcomingEvents = await _groupActivityService.GetGroupActivitiesAsync(
                    groupId: null,
                    startDate: DateTime.Today,
                    endDate: DateTime.Today.AddDays(30));
                upcomingEvents = upcomingEvents.OrderBy(e => e.Date).ToList();
                int eventsTotal = upcomingEvents.Count();
                var pagedEventsList = upcomingEvents
                    .Skip((eventPageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                var pagedEvents = new PaginatedList<GroupActivity>(pagedEventsList, eventsTotal, eventPageNumber, pageSize);

                // Student Progress with Pagination
                var studentsQuery = _studentService.GetStudentsQueryable(s => true);
                if (User.IsInRole("Teacher") || User.IsInRole("HeadTeacher") || User.IsInRole("ParishPriest"))
                {
                    studentsQuery = studentsQuery.OrderByDescending(s => s.AcademicYear);
                }
                else if (User.IsInRole("Parent") && familyMember != null)
                {
                    studentsQuery = studentsQuery.Where(s => s.FamilyMember.FamilyId == familyMember.FamilyId);
                }
                else
                {
                    studentsQuery = studentsQuery.Where(s => false); // No students for other roles
                }
                int studentsTotal = await studentsQuery.CountAsync();
                var studentProgress = await studentsQuery
                    .Skip((studentPageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                var pagedStudents = new PaginatedList<Student>(studentProgress, studentsTotal, studentPageNumber, pageSize);

                var model = new DashboardViewModel
                {
                    RecentAnnouncements = pagedAnnouncements,
                    UpcomingEvents = pagedEvents,
                    StudentProgress = pagedStudents
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load dashboard: {ex.Message}";
                return View(new DashboardViewModel());
            }
        }
    }
}