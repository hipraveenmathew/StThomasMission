using Microsoft.AspNetCore.Identity;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task CreateUserAsync(string email, string fullName, int wardId, string password, string role)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email is required.", nameof(email));
            if (string.IsNullOrEmpty(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password is required.", nameof(password));
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Role is required.", nameof(role));

            await _unitOfWork.Wards.GetByIdAsync(wardId);

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email {email} already exists.");

            if (!await _roleManager.RoleExistsAsync(role))
                throw new ArgumentException($"Role {role} does not exist.", nameof(role));

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                WardId = wardId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, role);

            await _auditService.LogActionAsync("System", "Create", nameof(ApplicationUser), user.Id, $"Created user: {email} with role {role}");
        }

        public async Task UpdateUserAsync(string userId, string fullName, int wardId, string? designation)
        {
            var user = await GetUserByIdAsync(userId);

            if (string.IsNullOrEmpty(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));

            await _unitOfWork.Wards.GetByIdAsync(wardId);

            user.FullName = fullName;
            user.WardId = wardId;
            user.Designation = designation;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _auditService.LogActionAsync("System", "Update", nameof(ApplicationUser), userId, $"Updated user: {fullName}");
        }

        public async Task AssignRoleAsync(string userId, string role)
        {
            var user = await GetUserByIdAsync(userId);

            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Role is required.", nameof(role));

            if (!await _roleManager.RoleExistsAsync(role))
                throw new ArgumentException($"Role {role} does not exist.", nameof(role));

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(role))
                return;

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            await _auditService.LogActionAsync("System", "Update", nameof(ApplicationUser), userId, $"Assigned role {role} to user: {user.FullName}");
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role)
        {
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Role is required.", nameof(role));

            if (!await _roleManager.RoleExistsAsync(role))
                throw new ArgumentException($"Role {role} does not exist.", nameof(role));

            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            return usersInRole.Where(u => u.IsActive);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new ArgumentException("User not found.", nameof(userId));
            return user;
        }
    }
}