using Microsoft.AspNetCore.Http;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class ImportViewModel
    {
        public IFormFile File { get; set; }
        public string SuccessMessage { get; set; } = string.Empty;
    }
}