using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Families.Models;
using StThomasMission.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Web.Areas.Families.Controllers
{
    [Area("Families")]
    [Authorize(Roles = "ParishAdmin,ParishPriest")]
    public class MessageHistoryController : Controller
    {
        private readonly ICommunicationService _communicationService;

        public MessageHistoryController(ICommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(MessageHistoryFilterViewModel filter, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["MethodSortParm"] = sortOrder == "method" ? "method_desc" : "method";

            // Build the query with server-side filtering
            var messagesQuery = _communicationService.GetMessageHistoryQueryable(filter.SearchString);

            // Apply sorting
            messagesQuery = sortOrder switch
            {
                "date_desc" => messagesQuery.OrderByDescending(m => m.SentAt),
                "method" => messagesQuery.OrderBy(m => m.Method),
                "method_desc" => messagesQuery.OrderByDescending(m => m.Method),
                _ => messagesQuery.OrderBy(m => m.SentAt),
            };

            // Pagination
            int totalItems = await messagesQuery.CountAsync();
            var pagedMessages = await messagesQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var model = new PaginatedList<MessageLog>(pagedMessages, totalItems, pageNumber, pageSize);
            return View(new MessageHistoryIndexViewModel
            {
                Filter = filter,
                Messages = model
            });
        }
    }
}