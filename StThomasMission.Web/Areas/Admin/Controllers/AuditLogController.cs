using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Admin.Models;
using StThomasMission.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogController(IAuditService auditService, UserManager<ApplicationUser> userManager)
        {
            _auditService = auditService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AuditLogFilterViewModel filter, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["ActionSortParm"] = sortOrder == "action" ? "action_desc" : "action";

            // Build the query with server-side filtering
            var logsQuery = _auditService.GetAuditLogsQueryable(
                entityName: filter.EntityName,
                startDate: filter.StartDate,
                endDate: filter.EndDate
            );

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                var searchLower = filter.SearchString.ToLower();
                logsQuery = logsQuery.Where(l =>
                    l.Action.ToLower().Contains(searchLower) ||
                    l.EntityName.ToLower().Contains(searchLower) ||
                    l.Details.ToLower().Contains(searchLower));
            }

            // Apply sorting
            logsQuery = sortOrder switch
            {
                "date_desc" => logsQuery.OrderByDescending(l => l.Timestamp),
                "action" => logsQuery.OrderBy(l => l.Action),
                "action_desc" => logsQuery.OrderByDescending(l => l.Action),
                _ => logsQuery.OrderBy(l => l.Timestamp),
            };

            // Pagination
            var totalItems = await logsQuery.CountAsync();
            var pagedLogs = await logsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

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
            return View(new AuditLogIndexViewModel
            {
                Filter = filter,
                Logs = model
            });
        }
    }
}