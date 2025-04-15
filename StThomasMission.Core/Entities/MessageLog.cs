using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class MessageLog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Recipient is required.")]
        public string Recipient { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Method is required.")]
        public string Method { get; set; } = string.Empty; // SMS, Email, WhatsApp

        [Required(ErrorMessage = "Message type is required.")]
        public string MessageType { get; set; } = string.Empty; // Announcement, Notification

        [Required]
        public DateTime SentAt { get; set; }
    }
}