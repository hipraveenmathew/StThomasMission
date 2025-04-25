using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddActivity(string groupName, string activityName, int points)
        {
            if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(activityName) || points <= 0)
            {
                TempData["Error"] = "Please fill all fields with valid values.";
                return View();
            }

            await _catechismService.AddGroupActivityAsync(groupName, activityName, points);
            TempData["Success"] = "Group activity added successfully!";
            return RedirectToAction("AddActivity");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var activities = await _catechismService.GetAllGroupActivitiesAsync();
            return View(activities);
        }

        [HttpGet]
        public async Task<IActionResult> Participation(int groupActivityId)
        {
            var participants = await _catechismService.GetStudentParticipationAsync(groupActivityId);
            ViewBag.GroupActivityId = groupActivityId;
            return View(participants);
        }

        [HttpGet]
        public IActionResult RecordParticipation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordParticipation(int studentId, int groupActivityId)
        {
            if (studentId <= 0 || groupActivityId <= 0)
            {
                TempData["Error"] = "Invalid student or activity selected.";
                return View();
            }

            await _catechismService.RecordStudentGroupActivityAsync(studentId, groupActivityId);
            TempData["Success"] = "Student participation recorded!";
            return RedirectToAction("RecordParticipation");
        }

        [Authorize(Roles = "HeadTeacher")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var activity = await _catechismService.GetGroupActivityByIdAsync(id);
            if (activity == null) return NotFound();
            return View(activity);
        }

        [Authorize(Roles = "HeadTeacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupActivity model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _catechismService.UpdateGroupActivityAsync(model);
            TempData["Success"] = "Activity updated!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "HeadTeacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _catechismService.DeleteGroupActivityAsync(id);
            TempData["Success"] = "Activity deleted!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ListActivities()
        {
            var activities = await _catechismService.GetAllGroupActivitiesAsync();
            return View(activities);
        }
    }
}
