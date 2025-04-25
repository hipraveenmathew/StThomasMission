using System;

namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // e.g., "Admin", "Teacher", "Parent"
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}