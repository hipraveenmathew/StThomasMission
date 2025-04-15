using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin,ParishPriest")]
    public class MessageHistoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageHistoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.MethodSortParm = sortOrder == "method" ? "method_desc" : "method";

            var messages = await _unitOfWork._context.MessageLogs.ToListAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                messages = messages.Where(m => m.Recipient.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                               m.Message.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            messages = sortOrder switch
            {
                "date_desc" => messages.OrderByDescending(m => m.SentAt).ToList(),
                "method" => messages.OrderBy(m => m.Method).ToList(),
                "method_desc" => messages.OrderByDescending(m => m.Method).ToList(),
                _ => messages.OrderBy(m => m.SentAt).ToList(),
            };

            int totalItems = messages.Count;
            var pagedMessages = messages.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var model = new PaginatedList<MessageLog>(pagedMessages, totalItems, pageNumber, pageSize);
            ViewBag.SearchString = searchString;
            return View(model);
        }
    }
}