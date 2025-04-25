using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum StudentStatus
    {
        [Display(Name = "Active")]
        Active,
        [Display(Name = "Graduated")]
        Graduated,
        [Display(Name = "Migrated")]
        Migrated,
        [Display(Name = "Inactive")]
        Inactive, // Added for temporarily inactive students
        [Display(Name = "Deleted")]
        Deleted  // Added for soft-deleted students
    }

}