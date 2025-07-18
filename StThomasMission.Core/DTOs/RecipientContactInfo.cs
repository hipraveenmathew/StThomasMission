namespace StThomasMission.Core.DTOs
{
    // A lightweight DTO for fetching contact info efficiently
    public class RecipientContactInfo
    {
        public string FirstName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        // Used for personalizing messages to parents
        public string? StudentName { get; set; }
    }
}