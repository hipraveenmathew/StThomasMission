using StThomasMission.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class BroadcastRequest
    {
        [Required]
        public int TargetId { get; set; } // e.g., WardId or GroupId

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public CommunicationChannel Channel { get; set; }
    }
}