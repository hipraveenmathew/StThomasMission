using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class MessageLog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Recipient is required.")]
        [MaxLength(150)]
        public string Recipient { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required.")]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Method is required.")]
        public CommunicationChannel Method { get; set; }  // SMS, Email, WhatsApp

        [Required(ErrorMessage = "Message type is required.")]
        public MessageType MessageType { get; set; }  // Announcement, Notification

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
