using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Web.Areas.Admin.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new UserRoleIndexViewModel
            {
                Users = users.Select(u => new UserRoleViewModel
                {
                    UserId = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Roles = _userManager.GetRolesAsync(u).Result.ToList(),
                    AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var model = new AssignRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList(),
                CurrentRoles = await _userManager.GetRolesAsync(user)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrEmpty(model.SelectedRole))
            {
                ModelState.AddModelError("SelectedRole", "Please select a role to assign.");
            }
            else
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.SelectedRole);
                if (!roleExists)
                {
                    ModelState.AddModelError("SelectedRole", "Selected role does not exist.");
                }
                else
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(model.SelectedRole))
                    {
                        var result = await _userManager.AddToRoleAsync(user, model.SelectedRole);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                model.Email = user.Email;
                model.FullName = user.FullName;
                model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                model.CurrentRoles = await _userManager.GetRolesAsync(user);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (!result.Succeeded)
            {
                TempData["Error"] = $"Failed to remove role {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }
            else
            {
                TempData["Success"] = $"Role {role} removed successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "Role name is required.";
                return RedirectToAction(nameof(Index));
            }

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["Error"] = $"Role '{roleName}' already exists.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                TempData["Error"] = $"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }
            else
            {
                TempData["Success"] = $"Role '{roleName}' created successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}