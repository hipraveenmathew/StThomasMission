using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class EditGroupActivityViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid group ID.")]
        public int GroupId { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "Activity name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Points must be a positive number.")]
        public int Points { get; set; }
    }
}