using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum FamilyMemberRole
    {
        [Display(Name = "Parent")]
        Parent,
        [Display(Name = "Child")]
        Child,
        [Display(Name = "Guardian")]
        Guardian,
        [Display(Name = "Other")]
        Other
    }
}