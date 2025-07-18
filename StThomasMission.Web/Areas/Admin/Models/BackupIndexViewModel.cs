using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class BackupIndexViewModel
    {
        public List<BackupViewModel> Backups { get; set; } = new List<BackupViewModel>();
    }
}