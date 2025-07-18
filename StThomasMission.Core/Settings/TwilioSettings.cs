namespace StThomasMission.Core.Models.Settings
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string SenderPhoneNumber { get; set; } = string.Empty;
        public string WhatsAppSenderPhoneNumber { get; set; } = string.Empty;
    }
}