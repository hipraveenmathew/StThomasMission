using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Catechism.Models;
using StThomasMission.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Catechism.Controllers
{
    [Area("Catechism")]
    [Authorize(Roles = "Teacher,HeadTeacher")]
    public class GroupActivitiesController : Controller
    {
        private readonly IGroupActivityService _groupActivityService; // Changed to IGroupActivityService

        public GroupActivitiesController(IGroupActivityService groupActivityService)
        {
            _groupActivityService = groupActivityService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var activities = await _groupActivityService.GetGroupActivitiesAsync();
                var pagedActivities = activities
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new PaginatedList<GroupActivity>(pagedActivities, activities.Count(), pageNumber, pageSize);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load activities: {ex.Message}";
                return View(new PaginatedList<GroupActivity>(new List<GroupActivity>(), 0, pageNumber, pageSize));
            }
        }

        [HttpGet]
        public IActionResult AddActivity()
        {
            return View(new AddGroupActivityViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddActivity(AddGroupActivityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _groupActivityService.AddGroupActivityAsync(
                    model.Name,
                    model.Description,
                    model.Date,
                    model.GroupId,
                    model.Points);
                TempData["Success"] = "Group activity added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to add activity: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Participation(int groupActivityId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var groupActivity = await _groupActivityService.GetGroupActivityByIdAsync(groupActivityId);
                if (groupActivity == null)
                {
                    TempData["Error"] = "Group activity not found.";
                    return RedirectToAction("Index");
                }

                var participants = await _groupActivityService.GetStudentGroupActivitiesAsync(groupActivityId);
                var pagedParticipants = participants
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new PaginatedList<StudentGroupActivity>(pagedParticipants, participants.Count(), pageNumber, pageSize);
                ViewData["GroupActivityId"] = groupActivityId;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load participants: {ex.Message}";
                return View(new PaginatedList<StudentGroupActivity>(new List<StudentGroupActivity>(), 0, pageNumber, pageSize));
            }
        }

        [HttpGet]
        public IActionResult RecordParticipation()
        {
            return View(new RecordParticipationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordParticipation(RecordParticipationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _groupActivityService.AddStudentToGroupActivityAsync(
                    model.StudentId,
                    model.GroupActivityId,
                    DateTime.UtcNow, // Use current date for participation
                    0); // PointsEarned can be set later or adjusted as needed
                model.SuccessMessage = "Student participation recorded successfully!";
                model.StudentId = 0;
                model.GroupActivityId = 0;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to record participation: {ex.Message}");
            }

            return View(model);
        }

        [Authorize(Roles = "HeadTeacher")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var activity = await _groupActivityService.GetGroupActivityByIdAsync(id);
                if (activity == null)
                {
                    return NotFound("Group activity not found.");
                }

                var model = new EditGroupActivityViewModel
                {
                    Id = activity.Id,
                    GroupId = activity.GroupId,
                    Name = activity.Name,
                    Date = activity.Date,
                    Description = activity.Description,
                    Points = activity.Points
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load activity: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "HeadTeacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditGroupActivityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _groupActivityService.UpdateGroupActivityAsync(
                    model.Id,
                    model.Name,
                    model.Description,
                    model.Date,
                    model.GroupId,
                    model.Points,
                    Core.Enums.ActivityStatus.Active);
                TempData["Success"] = "Activity updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to update activity: {ex.Message}");
                return View(model);
            }
        }

        [Authorize(Roles = "HeadTeacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _groupActivityService.DeleteGroupActivityAsync(id);
                TempData["Success"] = "Activity deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete activity: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}