using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StThomasMission.Core.Entities
{
    public class Ward
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ward name is required.")]
        [StringLength(100, ErrorMessage = "Ward name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public ICollection<Family> Families { get; set; } = new List<Family>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
