using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services
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

        public async Task AddGroupAsync(string name, string? description)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Group name is required.", nameof(name));

            var existing = await _unitOfWork.Groups.GetByNameAsync(name);
            if (existing != null)
                throw new InvalidOperationException($"Group '{name}' already exists.");

            var group = new Group
            {
                Name = name,
                Description = description,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };

            await _unitOfWork.Groups.AddAsync(group);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Group), group.Id.ToString(), $"Added group: {name}");
        }

        public async Task UpdateGroupAsync(int groupId, string name, string? description)
        {
            var group = await GetGroupByIdAsync(groupId);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Group name is required.", nameof(name));

            var existing = await _unitOfWork.Groups.GetByNameAsync(name);
            if (existing != null && existing.Id != groupId)
                throw new InvalidOperationException($"Group '{name}' already exists.");

            group.Name = name;
            group.Description = description;
            group.UpdatedDate = DateTime.UtcNow;
            group.UpdatedBy = "System";

            await _unitOfWork.Groups.UpdateAsync(group);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Group), groupId.ToString(), $"Updated group: {name}");
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            var group = await GetGroupByIdAsync(groupId);

            var students = await _unitOfWork.Students.GetByGroupIdAsync(groupId);
            if (students.Any())
                throw new InvalidOperationException("Cannot delete group with enrolled students.");

            var activities = await _unitOfWork.GroupActivities.GetByGroupIdAsync(groupId);
            if (activities.Any())
                throw new InvalidOperationException("Cannot delete group with associated activities.");

            await _unitOfWork.Groups.DeleteAsync(groupId);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Group), groupId.ToString(), $"Deleted group: {group.Name}");
        }

        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId);
            if (group == null)
                throw new ArgumentException("Group not found.", nameof(groupId));
            return group;
        }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync()
        {
            return await _unitOfWork.Groups.GetAllAsync();
        }
    }
}