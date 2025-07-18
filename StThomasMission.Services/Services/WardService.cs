using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class WardService : IWardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager; // Dependency was missing

        public WardService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager) // Injected here
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _userManager = userManager; // Stored here
        }

        public async Task<WardDetailDto> GetWardByIdAsync(int wardId)
        {
            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null)
            {
                throw new NotFoundException(nameof(Ward), wardId);
            }
            // Map to DTO
            return new WardDetailDto
            {
                Id = ward.Id,
                Name = ward.Name,
                FamilyCount = await _unitOfWork.Families.CountAsync(f => f.WardId == wardId)
            };
        }

        public async Task<IEnumerable<WardDetailDto>> GetAllWardsAsync()
        {
            return await _unitOfWork.Wards.GetAllWithDetailsAsync();
        }

        public async Task<WardDetailDto> CreateWardAsync(CreateWardRequest request, string userId)
        {
            var existing = await _unitOfWork.Wards.GetByNameAsync(request.Name);
            if (existing != null)
            {
                throw new InvalidOperationException($"A ward with the name '{request.Name}' already exists.");
            }

            var ward = new Ward
            {
                Name = request.Name,
                CreatedBy = userId
            };

            var newWard = await _unitOfWork.Wards.AddAsync(ward);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Create", nameof(Ward), newWard.Id.ToString(), $"Created ward '{newWard.Name}'.");

            return (await _unitOfWork.Wards.GetByNameAsync(newWard.Name))!;
        }

        public async Task UpdateWardAsync(int wardId, UpdateWardRequest request, string userId)
        {
            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null) throw new NotFoundException(nameof(Ward), wardId);

            var existingByName = await _unitOfWork.Wards.GetByNameAsync(request.Name);
            if (existingByName != null && existingByName.Id != wardId)
            {
                throw new InvalidOperationException($"A ward with the name '{request.Name}' already exists.");
            }

            ward.Name = request.Name;
            ward.UpdatedBy = userId;
            ward.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Wards.UpdateAsync(ward);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(Ward), wardId.ToString(), $"Updated ward to name '{ward.Name}'.");
        }

        public async Task DeleteWardAsync(int wardId, string userId)
        {
            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null) throw new NotFoundException(nameof(Ward), wardId);

            // Efficiently check for dependencies without loading full lists
            bool hasFamilies = await _unitOfWork.Families.AnyAsync(f => f.WardId == wardId);
            if (hasFamilies)
            {
                throw new InvalidOperationException("Cannot delete a ward with associated families. Please reassign the families first.");
            }

            // Corrected: Use the injected UserManager to query users.
            bool hasUsers = await _userManager.Users.AnyAsync(u => u.WardId == wardId);
            if (hasUsers)
            {
                throw new InvalidOperationException("Cannot delete a ward with associated users. Please reassign the users first.");
            }

            ward.IsDeleted = true;
            ward.UpdatedBy = userId;
            ward.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Wards.UpdateAsync(ward);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(Ward), wardId.ToString(), $"Soft-deleted ward '{ward.Name}'.");
        }
    }
}