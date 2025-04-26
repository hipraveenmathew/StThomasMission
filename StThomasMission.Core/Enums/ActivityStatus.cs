using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum ActivityStatus
    {
        [Display(Name = "Active")]
        Active,
        [Display(Name = "Inactive")]
        Inactive
    }
}