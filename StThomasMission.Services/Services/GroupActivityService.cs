using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    /// <summary>
    /// Service for managing group activities and points.
    /// </summary>
    public class GroupActivityService : IGroupActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatechismService _catechismService;

        public GroupActivityService(IUnitOfWork unitOfWork, ICatechismService catechismService)
        {
            _unitOfWork = unitOfWork;
            _catechismService = catechismService;
        }

        public async Task AddGroupActivityAsync(string group, string activityName, int points, DateTime date)
        {
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("Group is required.", nameof(group));
            if (string.IsNullOrEmpty(activityName))
                throw new ArgumentException("Activity name is required.", nameof(activityName));
            if (points < 0)
                throw new ArgumentException("Points cannot be negative.", nameof(points));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Activity date cannot be in the future.", nameof(date));

            var students = await _catechismService.GetStudentsByGroupAsync(group);
            if (!students.Any())
                throw new ArgumentException("No students found for the specified group.", nameof(group));

            var groupActivity = new GroupActivity
            {
                Group = group,
                ActivityName = activityName,
                Points = points,
                Date = date
            };

            await _unitOfWork.GroupActivities.AddAsync(groupActivity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateGroupActivityAsync(int activityId, string activityName, int points, DateTime date)
        {
            var groupActivity = await GetGroupActivityByIdAsync(activityId);

            if (string.IsNullOrEmpty(activityName))
                throw new ArgumentException("Activity name is required.", nameof(activityName));
            if (points < 0)
                throw new ArgumentException("Points cannot be negative.", nameof(points));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Activity date cannot be in the future.", nameof(date));

            groupActivity.ActivityName = activityName;
            groupActivity.Points = points;
            groupActivity.Date = date;

            await _unitOfWork.GroupActivities.UpdateAsync(groupActivity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteGroupActivityAsync(int activityId)
        {
            var groupActivity = await GetGroupActivityByIdAsync(activityId);
            await _unitOfWork.GroupActivities.DeleteAsync(groupActivity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<GroupActivity> GetGroupActivityByIdAsync(int activityId)
        {
            var groupActivity = await _unitOfWork.GroupActivities.GetByIdAsync(activityId);
            if (groupActivity == null)
                throw new ArgumentException("Group activity not found.", nameof(activityId));
            return groupActivity;
        }

        public async Task<IEnumerable<GroupActivity>> GetGroupActivitiesByGroupAsync(string group)
        {
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("Group is required.", nameof(group));

            var students = await _catechismService.GetStudentsByGroupAsync(group);
            if (!students.Any())
                throw new ArgumentException("No students found for the specified group.", nameof(group));

            return await _unitOfWork.GroupActivities.GetByGroupAsync(group);
        }

        public async Task<int> GetGroupPointsAsync(string group)
        {
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("Group is required.", nameof(group));

            var students = await _catechismService.GetStudentsByGroupAsync(group);
            if (!students.Any())
                throw new ArgumentException("No students found for the specified group.", nameof(group));

            var activities = await _unitOfWork.GroupActivities.GetByGroupAsync(group);
            return activities.Sum(a => a.Points);
        }
    }
}