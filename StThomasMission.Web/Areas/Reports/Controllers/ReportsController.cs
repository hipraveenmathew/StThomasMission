using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Web.Areas.Reports.Models;

namespace StThomasMission.Web.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Authorize(Roles = "HeadTeacher,ParishAdmin")]
    public class ReportsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var model = new ReportsIndexViewModel
            {
                CurrentAcademicYear = DateTime.UtcNow.Year
            };
            return View(model);
        }
    }
}