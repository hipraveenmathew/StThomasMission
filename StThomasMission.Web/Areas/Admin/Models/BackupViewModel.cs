using System;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class BackupViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSizeInKB { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}