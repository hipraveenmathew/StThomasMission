using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StThomasMission.Core.Constants;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using StThomasMission.Web.Areas.Admin.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class RolesController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IUserService userService, ILogger<RolesController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int pageNumber = 1, int pageSize = 15)
        {
            var pagedUsers = await _userService.SearchUsersPaginatedAsync(pageNumber, pageSize, searchTerm);
            var allRoles = await _userService.GetAllRolesAsync();

            var model = new UserRoleIndexViewModel
            {
                PagedUsers = pagedUsers,
                RolesForDropdown = new SelectList(allRoles),
                SearchTerm = searchTerm
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "User ID and Role must be provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _userService.UpdateUserRoleAsync(userId, role, performedByUserId);
                TempData["Success"] = "User role updated successfully.";
            }
            catch (NotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role for UserId {UserId}", userId);
                TempData["Error"] = "An unexpected error occurred while updating the role.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}