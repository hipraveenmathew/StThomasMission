using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.ActionSortParm = sortOrder == "action" ? "action_desc" : "action";

            var logs = await _unitOfWork._context.AuditLogs.ToListAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                logs = logs.Where(l => l.Action.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                       l.EntityName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                       l.Details.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            logs = sortOrder switch
            {
                "date_desc" => logs.OrderByDescending(l => l.Timestamp).ToList(),
                "action" => logs.OrderBy(l => l.Action).ToList(),
                "action_desc" => logs.OrderByDescending(l => l.Action).ToList(),
                _ => logs.OrderBy(l => l.Timestamp).ToList(),
            };

            int totalItems = logs.Count;
            var pagedLogs = logs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var model = new PaginatedList<AuditLog>(pagedLogs, totalItems, pageNumber, pageSize);
            ViewBag.SearchString = searchString;
            return View(model);
        }
    }
}