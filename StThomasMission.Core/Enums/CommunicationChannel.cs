using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum CommunicationChannel
    {
        [Display(Name = "SMS")]
        SMS,
        [Display(Name = "Email")]
        Email,
        [Display(Name = "WhatsApp")]
        WhatsApp
    }
}
