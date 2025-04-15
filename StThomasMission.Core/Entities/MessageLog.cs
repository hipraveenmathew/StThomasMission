﻿using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class MessageLog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Recipient is required.")]
        [StringLength(150, ErrorMessage = "Recipient name or address cannot exceed 150 characters.")]
        public string Recipient { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Method is required.")]
        public CommunicationChannel Method { get; set; }

        [Required(ErrorMessage = "Message type is required.")]
        public MessageType MessageType { get; set; }

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
