using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum AttendanceStatus
    {
        [Display(Name = "Present")]
        Present,
        [Display(Name = "Absent")]
        Absent
    }
}