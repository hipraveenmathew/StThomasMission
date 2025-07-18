using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class GroupActivityService : IGroupActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public GroupActivityService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<GroupActivityDto> CreateGroupActivityAsync(CreateGroupActivityRequest request, string userId)
        {
            if (await _unitOfWork.Groups.GetByIdAsync(request.GroupId) == null)
            {
                throw new NotFoundException(nameof(Group), request.GroupId);
            }

            var groupActivity = new GroupActivity
            {
                Name = request.Name,
                Description = request.Description,
                Date = request.Date,
                GroupId = request.GroupId,
                Points = request.Points,
                Status = request.Status,
                CreatedBy = userId
            };

            var newActivity = await _unitOfWork.GroupActivities.AddAsync(groupActivity);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Create", nameof(GroupActivity), newActivity.Id.ToString(), $"Created group activity '{newActivity.Name}'.");

            // Corrected: Manually map the entity to the DTO
            var activityDto = await _unitOfWork.GroupActivities.GetByGroupIdAsync(newActivity.GroupId);
            return activityDto.First(a => a.Id == newActivity.Id);
        }

        public async Task UpdateGroupActivityAsync(int groupActivityId, UpdateGroupActivityRequest request, string userId)
        {
            var activity = await _unitOfWork.GroupActivities.GetByIdAsync(groupActivityId);
            if (activity == null) throw new NotFoundException(nameof(GroupActivity), groupActivityId);

            activity.Name = request.Name;
            activity.Description = request.Description;
            activity.Date = request.Date;
            activity.Points = request.Points;
            activity.Status = request.Status;
            activity.UpdatedBy = userId;
            activity.UpdatedAt = DateTime.UtcNow; // This now compiles

            await _unitOfWork.GroupActivities.UpdateAsync(activity);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(GroupActivity), groupActivityId.ToString(), $"Updated group activity '{activity.Name}'.");
        }

        public async Task DeleteGroupActivityAsync(int groupActivityId, string userId)
        {
            var activity = await _unitOfWork.GroupActivities.GetByIdAsync(groupActivityId);
            if (activity == null) throw new NotFoundException(nameof(GroupActivity), groupActivityId);

            var participants = await _unitOfWork.StudentGroupActivities.GetByGroupActivityIdAsync(groupActivityId);
            if (participants.Any())
            {
                throw new InvalidOperationException("Cannot delete an activity that has student participants. Please mark it as Inactive instead.");
            }

            activity.IsDeleted = true;
            activity.UpdatedBy = userId;
            activity.UpdatedAt = DateTime.UtcNow; // This now compiles

            await _unitOfWork.GroupActivities.UpdateAsync(activity);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(GroupActivity), groupActivityId.ToString(), $"Soft-deleted group activity '{activity.Name}'.");
        }

        public async Task AssignStudentsToActivityAsync(int groupActivityId, AssignStudentsToActivityRequest request, string userId)
        {
            var activity = await _unitOfWork.GroupActivities.GetByIdAsync(groupActivityId);
            if (activity == null) throw new NotFoundException(nameof(GroupActivity), groupActivityId);

            // This now compiles
            var existingParticipantIds = await _unitOfWork.StudentGroupActivities.GetParticipantsByIdsAsync(groupActivityId, request.StudentIds);
            if (existingParticipantIds.Any())
            {
                throw new InvalidOperationException($"One or more students are already assigned to this activity. (e.g., Student ID: {existingParticipantIds.First()})");
            }

            foreach (var studentId in request.StudentIds)
            {
                var assignment = new StudentGroupActivity
                {
                    StudentId = studentId,
                    GroupActivityId = groupActivityId,
                    ParticipationDate = request.ParticipationDate,
                    PointsEarned = activity.Points,
                    Remarks = request.Remarks,
                    RecordedBy = userId
                };
                await _unitOfWork.StudentGroupActivities.AddAsync(assignment);
            }

            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "AssignStudents", nameof(GroupActivity), groupActivityId.ToString(), $"Assigned {request.StudentIds.Count} students to activity '{activity.Name}'.");
        }
    }
}