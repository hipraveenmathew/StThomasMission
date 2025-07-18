using Microsoft.Extensions.Options;
using StThomasMission.Core.Models.Settings;
using StThomasMission.Services.Interfaces;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace StThomasMission.Services.Messaging
{
    public class TwilioSmsSender : ISmsSender
    {
        private readonly TwilioSettings _settings;

        public TwilioSmsSender(IOptions<TwilioSettings> settings)
        {
            _settings = settings.Value;
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
        }

        public async Task<(string Status, string? Details)> SendSmsAsync(string toNumber, string message)
        {
            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_settings.SenderPhoneNumber),
                to: new PhoneNumber(toNumber)
            );
            return (result.Status.ToString(), result.ErrorMessage);
        }

        public async Task<(string Status, string? Details)> SendWhatsAppAsync(string toNumber, string message)
        {
            var result = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber($"whatsapp:{_settings.WhatsAppSenderPhoneNumber}"),
                to: new PhoneNumber($"whatsapp:{toNumber}")
            );
            return (result.Status.ToString(), result.ErrorMessage);
        }
    }
}