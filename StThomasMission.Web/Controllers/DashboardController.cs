using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Admin.Models;
using StThomasMission.Web.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.ParishPriest},{UserRoles.HeadTeacher},{UserRoles.Teacher}")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var summary = await _dashboardService.GetDashboardSummaryAsync(userId);

            var model = new DashboardViewModel
            {
                TotalStudents = summary.TotalStudents,
                ActiveStudents = summary.ActiveStudents,
                GraduatedStudents = summary.GraduatedStudents,
                TotalFamilies = summary.TotalFamilies,
                RegisteredFamilies = summary.RegisteredFamilies,
                TotalWards = summary.TotalWards,
                TotalGroups = summary.TotalGroups,
                RecentAnnouncements = summary.RecentAnnouncements,
                UpcomingEvents = summary.UpcomingEvents
            };

            return View(model);
        }
    }
}