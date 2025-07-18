using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StThomasMission.Core.Constants;
using StThomasMission.Core.DTOs;
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
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWardService _wardService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IWardService wardService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _wardService = wardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int pageNumber = 1, int pageSize = 15)
        {
            var pagedUsers = await _userService.SearchUsersPaginatedAsync(pageNumber, pageSize, searchTerm);
            var model = new UserIndexViewModel
            {
                PagedUsers = pagedUsers,
                SearchTerm = searchTerm
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserFormViewModel
            {
                AvailableWards = await GetWardsSelectList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableWards = await GetWardsSelectList();
                return View(model);
            }

            try
            {
                var request = new CreateUserRequest
                {
                    Email = model.Email,
                    Password = model.Password,
                    FullName = model.FullName,
                    WardId = model.WardId,
                    Role = model.Role
                };
                var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _userService.CreateUserAsync(request, performedByUserId);

                TempData["Success"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableWards = await GetWardsSelectList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var userDto = await _userService.GetUserByIdAsync(id);
                var model = new UserFormViewModel
                {
                    Id = userDto.Id,
                    Email = userDto.Email,
                    FullName = userDto.FullName,
                    WardId = userDto.WardId ?? 0,
                    Designation = userDto.Designation,
                    Role = userDto.Role,
                    AvailableWards = await GetWardsSelectList()
                };
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableWards = await GetWardsSelectList();
                return View(model);
            }

            try
            {
                var request = new UpdateUserRequest
                {
                    FullName = model.FullName,
                    WardId = model.WardId,
                    Designation = model.Designation
                };
                var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _userService.UpdateUserAsync(model.Id, request, performedByUserId);

                TempData["Success"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", model.Id);
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableWards = await GetWardsSelectList();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                var performedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _userService.DeactivateUserAsync(id, performedByUserId);
                TempData["Success"] = "User has been deactivated.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deactivating user: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<SelectList> GetWardsSelectList()
        {
            var wards = await _wardService.GetAllWardsAsync();
            return new SelectList(wards, "Id", "Name");
        }
    }
}