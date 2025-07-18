using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Shared;
using StThomasMission.Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(nameof(ApplicationUser), userId);
            }
            return await MapUserToDtoAsync(user);
        }
        // Add these new methods to your UserService class
        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }
        // Add this method to your UserService class
        public async Task ReactivateUserAsync(string userId, string performedByUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException(nameof(ApplicationUser), userId);

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            await _auditService.LogActionAsync(performedByUserId, "Reactivate", nameof(ApplicationUser), userId, $"Reactivated user '{user.Email}'.");
        }

        public async Task UpdateUserRoleAsync(string userId, string newRole, string performedByUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException("User", userId);

            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                throw new NotFoundException("Role", newRole);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new AppException($"Failed to remove existing roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                throw new AppException($"Failed to add new role: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
            }

            await _auditService.LogActionAsync(performedByUserId, "UpdateRole", nameof(ApplicationUser), user.Id, $"Changed role for user '{user.Email}' to {newRole}.");
        }

        public async Task<IPaginatedList<UserDto>> SearchUsersPaginatedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? role = null)
        {
            var query = _userManager.Users;

            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIdsInRole = usersInRole.Select(u => u.Id).ToList();
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.FullName.Contains(searchTerm) || (u.Email != null && u.Email.Contains(searchTerm)));
            }

            var pagedUsers = await query.OrderBy(u => u.FullName)
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in pagedUsers)
            {
                userDtos.Add(await MapUserToDtoAsync(user));
            }

            var totalCount = await query.CountAsync();

            return new PaginatedList<UserDto>(userDtos, totalCount, pageNumber, pageSize);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request, string performedByUserId)
        {
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                throw new AppException($"User with email '{request.Email}' already exists.");
            }
            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                throw new NotFoundException(nameof(IdentityRole), request.Role);
            }
            if (await _unitOfWork.Wards.GetByIdAsync(request.WardId) == null)
            {
                throw new NotFoundException(nameof(Ward), request.WardId);
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                WardId = request.WardId,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new AppException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.AddToRoleAsync(user, request.Role);
            await _auditService.LogActionAsync(performedByUserId, "Create", nameof(ApplicationUser), user.Id, $"Created user '{user.Email}' with role {request.Role}.");

            return await GetUserByIdAsync(user.Id);
        }

        public async Task UpdateUserAsync(string userId, UpdateUserRequest request, string performedByUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException(nameof(ApplicationUser), userId);

            if (user.WardId != request.WardId && await _unitOfWork.Wards.GetByIdAsync(request.WardId) == null)
            {
                throw new NotFoundException(nameof(Ward), request.WardId);
            }

            user.FullName = request.FullName;
            user.WardId = request.WardId;
            user.Designation = request.Designation;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new AppException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _auditService.LogActionAsync(performedByUserId, "Update", nameof(ApplicationUser), userId, $"Updated details for user '{user.Email}'.");
        }

        public async Task DeactivateUserAsync(string userId, string performedByUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new NotFoundException(nameof(ApplicationUser), userId);

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            await _auditService.LogActionAsync(performedByUserId, "Deactivate", nameof(ApplicationUser), userId, $"Deactivated user '{user.Email}'.");
        }

        private async Task<UserDto> MapUserToDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var ward = user.WardId.HasValue ? await _unitOfWork.Wards.GetByIdAsync(user.WardId.Value) : null;

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault(),
                WardId = user.WardId,
                WardName = ward?.Name,
                Designation = user.Designation,
                IsActive = user.IsActive
            };
        }
    }
}