using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public GroupService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<GroupDetailDto> GetGroupByIdAsync(int groupId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId);
            if (group == null)
            {
                throw new NotFoundException(nameof(Group), groupId);
            }

            // Map to DTO
            return new GroupDetailDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                StudentCount = await _unitOfWork.Students.CountAsync(s => s.GroupId == groupId)
            };
        }

        public async Task<IEnumerable<GroupDetailDto>> GetAllGroupsAsync()
        {
            return await _unitOfWork.Groups.GetAllWithDetailsAsync();
        }

        public async Task<GroupDetailDto> CreateGroupAsync(CreateGroupRequest request, string userId)
        {
            var existing = await _unitOfWork.Groups.GetByNameAsync(request.Name);
            if (existing != null)
            {
                throw new InvalidOperationException($"A group with the name '{request.Name}' already exists.");
            }

            var group = new Group
            {
                Name = request.Name,
                Description = request.Description,
                CreatedBy = userId
            };

            var newGroup = await _unitOfWork.Groups.AddAsync(group);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Create", nameof(Group), newGroup.Id.ToString(), $"Created group '{newGroup.Name}'.");

            return (await _unitOfWork.Groups.GetByNameAsync(newGroup.Name))!;
        }

        public async Task UpdateGroupAsync(int groupId, UpdateGroupRequest request, string userId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId);
            if (group == null) throw new NotFoundException(nameof(Group), groupId);

            var existingByName = await _unitOfWork.Groups.GetByNameAsync(request.Name);
            if (existingByName != null && existingByName.Id != groupId)
            {
                throw new InvalidOperationException($"A group with the name '{request.Name}' already exists.");
            }

            group.Name = request.Name;
            group.Description = request.Description;
            group.UpdatedBy = userId;
            group.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Groups.UpdateAsync(group);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(Group), groupId.ToString(), $"Updated group '{group.Name}'.");
        }

        public async Task DeleteGroupAsync(int groupId, string userId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId);
            if (group == null) throw new NotFoundException(nameof(Group), groupId);

            // Efficiently check for dependencies without loading full lists
            bool hasStudents = await _unitOfWork.Students.AnyAsync(s => s.GroupId == groupId);
            if (hasStudents)
            {
                throw new InvalidOperationException("Cannot delete a group that has students assigned to it. Please reassign the students first.");
            }

            bool hasActivities = await _unitOfWork.GroupActivities.AnyAsync(ga => ga.GroupId == groupId);
            if (hasActivities)
            {
                throw new InvalidOperationException("Cannot delete a group with associated activities. Please delete the activities first.");
            }

            group.IsDeleted = true;
            group.UpdatedBy = userId;
            group.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Groups.UpdateAsync(group);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(Group), groupId.ToString(), $"Soft-deleted group '{group.Name}'.");
        }
    }
}