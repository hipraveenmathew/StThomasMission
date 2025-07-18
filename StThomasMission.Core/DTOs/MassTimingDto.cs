using StThomasMission.Core.Enums;
using System;
using System.Text.Json.Serialization;

namespace StThomasMission.Core.DTOs
{
    public class MassTimingDto
    {
        public int Id { get; set; }
        public string Day { get; set; } = string.Empty;
        public TimeSpan Time { get; set; }
        public string Location { get; set; } = string.Empty;
        public MassType Type { get; set; }

        [JsonIgnore] // This property is only for sorting, not for client-side display
        public DateTime WeekStartDate { get; set; }
    }
}