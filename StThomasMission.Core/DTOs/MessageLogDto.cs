using StThomasMission.Core.Enums;
using System;

namespace StThomasMission.Core.DTOs
{
    public class MessageLogDto
    {
        public int Id { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public CommunicationChannel Method { get; set; }
        public MessageType MessageType { get; set; }
        public DateTime SentAt { get; set; }
        public string? Status { get; set; }
        public string? ResponseDetails { get; set; }
        public string? SentBy { get; set; }
    }
}