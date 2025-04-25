using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Admin.Models;
using StThomasMission.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.ActionSortParm = sortOrder == "action" ? "action_desc" : "action";

            var logs = (await _unitOfWork.AuditLogs.GetAllAsync()).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                logs = logs.Where(l =>
                    l.Action.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    l.EntityName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    l.Details.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            logs = sortOrder switch
            {
                "date_desc" => logs.OrderByDescending(l => l.Timestamp),
                "action" => logs.OrderBy(l => l.Action),
                "action_desc" => logs.OrderByDescending(l => l.Action),
                _ => logs.OrderBy(l => l.Timestamp),
            };

            var totalItems = logs.Count();
            var pagedLogs = logs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Map to ViewModel with Username
            var viewModelList = new List<AuditLogViewModel>();
            foreach (var log in pagedLogs)
            {
                var user = await _userManager.FindByIdAsync(log.UserId);
                viewModelList.Add(new AuditLogViewModel
                {
                    Timestamp = log.Timestamp,
                    Action = log.Action,
                    EntityName = log.EntityName,
                    Details = log.Details,
                    Username = user?.UserName ?? "Unknown"
                });
            }

            var model = new PaginatedList<AuditLogViewModel>(viewModelList, totalItems, pageNumber, pageSize);
            ViewBag.SearchString = searchString;

            return View(model);
        }
    }
}
