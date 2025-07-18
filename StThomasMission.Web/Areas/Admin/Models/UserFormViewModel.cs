using Microsoft.AspNetCore.Mvc.Rendering;
using StThomasMission.Core.Constants;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class UserFormViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a ward.")]
        [Display(Name = "Ward")]
        public int WardId { get; set; }

        // Only required for Create action
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; } = UserRoles.Parent;

        [StringLength(100)]
        public string? Designation { get; set; }

        public bool IsActive { get; set; } = true;

        // For dropdown lists
        public SelectList? AvailableWards { get; set; }
        public SelectList AvailableRoles { get; set; } = new SelectList(new List<string>
        {
            UserRoles.Admin,
            UserRoles.ParishPriest,
            UserRoles.ParishAdmin,
            UserRoles.HeadTeacher,
            UserRoles.Teacher,
            UserRoles.Parent
        });
    }
}