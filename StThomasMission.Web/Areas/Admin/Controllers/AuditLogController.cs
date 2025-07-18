using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Constants;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Admin.Models;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AuditLogController : Controller
    {
        private readonly IAuditService _auditService;

        public AuditLogController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        // This is the updated Index action in your AuditLogController
        [HttpGet]
        public async Task<IActionResult> Index(AuditLogFilterViewModel filter, string? sortOrder, int pageNumber = 1, int pageSize = 20)
        {
            var pagedLogs = await _auditService.GetLogsAsync(
                pageNumber,
                pageSize,
                filter.UserId,
                filter.EntityName,
                sortOrder, // Pass sortOrder to the service
                filter.StartDate,
                filter.EndDate
            );

            var model = new AuditLogIndexViewModel
            {
                Filter = filter,
                Logs = pagedLogs,
                CurrentSort = sortOrder // Set the current sort on the view model
            };

            return View(model);
        }
    }
}