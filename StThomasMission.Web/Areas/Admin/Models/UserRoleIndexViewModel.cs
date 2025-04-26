using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class UserRoleIndexViewModel
    {
        public List<UserRoleViewModel> Users { get; set; } = new List<UserRoleViewModel>();
    }
}