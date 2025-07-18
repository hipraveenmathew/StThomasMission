using System.Text.RegularExpressions;

namespace StThomasMission.Web.Models
{
    public class ContactViewModel
    {
        public string AddressLine1 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string GoogleMapsEmbedUrl { get; set; } = string.Empty;

        // A helper property to format the phone number for display
        public string PhoneNumberFormatted
        {
            get
            {
                if (string.IsNullOrEmpty(PhoneNumber)) return string.Empty;
                // Example: formats +441614372861 to +44 (0) 161 437 2861
                var match = Regex.Match(PhoneNumber, @"\+44(\d{3})(\d{3})(\d{4})");
                return match.Success ? $"+44 (0) {match.Groups[1]} {match.Groups[2]} {match.Groups[3]}" : PhoneNumber;
            }
        }
    }
}