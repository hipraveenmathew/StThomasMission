using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class GroupActivityService : IGroupActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly IAuditService _auditService;

        public GroupActivityService(IUnitOfWork unitOfWork, IStudentService studentService, IGroupService groupService, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _groupService = groupService;
            _auditService = auditService;
        }

        public async Task AddGroupActivityAsync(string name, string description, DateTime date, int groupId, int points)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Activity name is required.", nameof(name));
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description is required.", nameof(description));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Activity date cannot be in the future.", nameof(date));
            if (points < 0)
                throw new ArgumentException("Points cannot be negative.", nameof(points));

            await _groupService.GetGroupByIdAsync(groupId);

            var groupActivity = new GroupActivity
            {
                Name = name,
                Description = description,
                Date = date,
                GroupId = groupId,
                Points = points,
                Status = ActivityStatus.Active,
                CreatedBy = "System"
            };

            await _unitOfWork.GroupActivities.AddAsync(groupActivity);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(GroupActivity), groupActivity.Id.ToString(), $"Added group activity: {name}");
        }

        public async Task UpdateGroupActivityAsync(int groupActivityId, string name, string description, DateTime date, int groupId, int points, ActivityStatus status)
        {
            var groupActivity = await GetGroupActivityByIdAsync(groupActivityId);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Activity name is required.", nameof(name));
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description is required.", nameof(description));
            if (date > DateTime.UtcNow)
                throw new ArgumentException("Activity date cannot be in the future.", nameof(date));
            if (points < 0)
                throw new ArgumentException("Points cannot be negative.", nameof(points));

            await _groupService.GetGroupByIdAsync(groupId);

            groupActivity.Name = name;
            groupActivity.Description = description;
            groupActivity.Date = date;
            groupActivity.GroupId = groupId;
            groupActivity.Points = points;
            groupActivity.Status = status;
            groupActivity.UpdatedBy = "System";

            await _unitOfWork.GroupActivities.UpdateAsync(groupActivity);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(GroupActivity), groupActivityId.ToString(), $"Updated group activity: {name}");
        }

        public async Task DeleteGroupActivityAsync(int groupActivityId)
        {
            var groupActivity = await GetGroupActivityByIdAsync(groupActivityId);

            var participants = await _unitOfWork.StudentGroupActivities.GetByGroupActivityIdAsync(groupActivityId);
            if (participants.Any())
                throw new InvalidOperationException("Cannot delete activity with participants.");

            await _unitOfWork.GroupActivities.DeleteAsync(groupActivityId);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(GroupActivity), groupActivityId.ToString(), $"Deleted group activity: {groupActivity.Name}");
        }

        public async Task AddStudentToGroupActivityAsync(int studentId, int groupActivityId, DateTime participationDate, int pointsEarned)
        {
            await _studentService.GetStudentByIdAsync(studentId);
            var groupActivity = await GetGroupActivityByIdAsync(groupActivityId);

            if (participationDate > DateTime.UtcNow)
                throw new ArgumentException("Participation date cannot be in the future.", nameof(participationDate));
            if (pointsEarned < 0)
                throw new ArgumentException("Points earned cannot be negative.", nameof(pointsEarned));

            var existing = await _unitOfWork.StudentGroupActivities.GetAsync(sga => sga.StudentId == studentId && sga.GroupActivityId == groupActivityId);
            if (existing.Any())
                throw new InvalidOperationException("Student is already participating in this activity.");

            var studentGroupActivity = new StudentGroupActivity
            {
                StudentId = studentId,
                GroupActivityId = groupActivityId,
                ParticipationDate = participationDate,
                PointsEarned = pointsEarned,
                RecordedBy = "System"
            };

            await _unitOfWork.StudentGroupActivities.AddAsync(studentGroupActivity);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(StudentGroupActivity), $"{studentId}-{groupActivityId}", $"Added student {studentId} to activity: {groupActivity.Name}");
        }

        public async Task<IEnumerable<GroupActivity>> GetGroupActivitiesAsync(int? groupId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (groupId.HasValue)
            {
                await _groupService.GetGroupByIdAsync(groupId.Value);
                return await _unitOfWork.GroupActivities.GetByGroupIdAsync(groupId.Value, startDate, endDate);
            }
            return await _unitOfWork.GroupActivities.GetAsync(ga =>
                (startDate == null || ga.Date >= startDate) &&
                (endDate == null || ga.Date <= endDate));
        }

        public async Task<IEnumerable<StudentGroupActivity>> GetStudentGroupActivitiesAsync(int studentId)
        {
            await _studentService.GetStudentByIdAsync(studentId);
            return await _unitOfWork.StudentGroupActivities.GetByStudentIdAsync(studentId);
        }

        public async Task<GroupActivity?> GetGroupActivityByIdAsync(int groupActivityId)
        {
            var groupActivity = await _unitOfWork.GroupActivities.GetByIdAsync(groupActivityId);
            if (groupActivity == null || groupActivity.Status == ActivityStatus.Inactive)
                throw new ArgumentException("Group activity not found.", nameof(groupActivityId));
            return groupActivity;
        }
    }
}