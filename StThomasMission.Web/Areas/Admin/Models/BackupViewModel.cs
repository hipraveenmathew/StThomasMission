using System;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class BackupViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}