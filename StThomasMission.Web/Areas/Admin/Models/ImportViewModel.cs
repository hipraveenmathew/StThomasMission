using Microsoft.AspNetCore.Http;
using StThomasMission.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class ImportViewModel
    {
        [Required(ErrorMessage = "Please select a file.")]
        [Display(Name = "Excel File (.xlsx)")]
        public IFormFile? File { get; set; }

        // This property will hold the results after an import attempt
        public ImportResultDto? Result { get; set; }
    }
}