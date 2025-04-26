using StThomasMission.Core.Entities;
using StThomasMission.Web.Models;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class MessageHistoryIndexViewModel
    {
        public MessageHistoryFilterViewModel Filter { get; set; } = new MessageHistoryFilterViewModel();
        public PaginatedList<MessageLog> Messages { get; set; }
    }
}