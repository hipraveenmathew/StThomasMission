using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class WardService : IWardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public WardService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _auditService = auditService;
        }

        public async Task<Ward> CreateWardAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Ward name is required.", nameof(name));

            var existingWard = await _unitOfWork.Wards.GetByNameAsync(name);
            if (existingWard != null)
                throw new InvalidOperationException($"Ward '{name}' already exists.");

            var ward = new Ward
            {
                Name = name,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };

            await _unitOfWork.Wards.AddAsync(ward);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Ward), ward.Id.ToString(), $"Created ward: {name}");

            return ward;
        }

        public async Task UpdateWardAsync(int wardId, string name)
        {
            var ward = await GetWardByIdAsync(wardId);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Ward name is required.", nameof(name));

            var existingWard = await _unitOfWork.Wards.GetByNameAsync(name);
            if (existingWard != null && existingWard.Id != wardId)
                throw new InvalidOperationException($"Ward '{name}' already exists.");

            ward.Name = name;
            ward.UpdatedDate = DateTime.UtcNow;
            ward.UpdatedBy = "System";

            await _unitOfWork.Wards.UpdateAsync(ward);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Ward), wardId.ToString(), $"Updated ward: {name}");
        }

        public async Task DeleteWardAsync(int wardId)
        {
            var ward = await GetWardByIdAsync(wardId);

            var families = await _unitOfWork.Families.GetByWardAsync(wardId);
            if (families.Any())
                throw new InvalidOperationException("Cannot delete ward with associated families.");

            var users = await _userManager.Users.Where(u => u.WardId == wardId).ToListAsync();
            if (users.Any())
                throw new InvalidOperationException("Cannot delete ward with associated users.");

            ward.IsDeleted = true;
            ward.UpdatedDate = DateTime.UtcNow;
            ward.UpdatedBy = "System";

            await _unitOfWork.Wards.UpdateAsync(ward);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Ward), wardId.ToString(), $"Soft-deleted ward: {ward.Name}");
        }

        public async Task<Ward?> GetWardByIdAsync(int wardId)
        {
            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null || ward.IsDeleted)
                throw new ArgumentException("Ward not found.", nameof(wardId));
            return ward;
        }

        public async Task<IEnumerable<Ward>> GetAllWardsAsync()
        {
            return await _unitOfWork.Wards.GetAsync(w => !w.IsDeleted);
        }
    }
}