namespace StThomasMission.Core.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? WardId { get; set; }
        public string? WardName { get; set; }
        public string? Designation { get; set; }
        public bool IsActive { get; set; }
    }
}