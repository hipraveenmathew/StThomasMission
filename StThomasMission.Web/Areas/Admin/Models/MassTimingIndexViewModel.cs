using StThomasMission.Core.DTOs;
using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class MassTimingIndexViewModel
    {
        public List<MassTimingDto> MassTimings { get; set; } = new List<MassTimingDto>();
    }
}