using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Interfaces;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "Teacher,HeadTeacher")]
    public class GroupActivitiesController : Controller
    {
        private readonly ICatechismService _catechismService;

        public GroupActivitiesController(ICatechismService catechismService)
        {
            _catechismService = catechismService;
        }

        [HttpGet]
        public IActionResult AddActivity()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddActivity(string groupName, string activityName, int points)
        {
            await _catechismService.AddGroupActivityAsync(groupName, activityName, points);
            TempData["Success"] = "Group activity added successfully!";
            return RedirectToAction("AddActivity");
        }

        [HttpGet]
        public IActionResult RecordParticipation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecordParticipation(int studentId, int groupActivityId)
        {
            await _catechismService.RecordStudentGroupActivityAsync(studentId, groupActivityId);
            TempData["Success"] = "Student participation recorded!";
            return RedirectToAction("RecordParticipation");
        }
    }
}