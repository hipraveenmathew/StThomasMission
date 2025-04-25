using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCrypt.Net;
using Org.BouncyCastle.Crypto.Generators;

namespace StThomasMission.Services
{
    /// <summary>
    /// Service for managing user accounts and roles.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task RegisterUserAsync(string username, string email, string password, string role)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username is required.", nameof(username));
            if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Valid email is required.", nameof(email));
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));
            if (string.IsNullOrEmpty(role) || (role != "Admin" && role != "Teacher" && role != "Parent"))
                throw new ArgumentException("Role must be 'Admin', 'Teacher', or 'Parent'.", nameof(role));

            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);
            if (existingUser != null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateUserAsync(int userId, string username, string email, string? password, string role, bool isActive)
        {
            var user = await GetUserByIdAsync(userId);

            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username is required.", nameof(username));
            if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Valid email is required.", nameof(email));
            if (!string.IsNullOrEmpty(password) && password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));
            if (string.IsNullOrEmpty(role) || (role != "Admin" && role != "Teacher" && role != "Parent"))
                throw new ArgumentException("Role must be 'Admin', 'Teacher', or 'Parent'.", nameof(role));

            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);
            if (existingUser != null && existingUser.Id != userId)
                throw new InvalidOperationException("Email is already registered.");

            user.Username = username;
            user.Email = email;
            if (!string.IsNullOrEmpty(password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.Role = role;
            user.IsActive = isActive;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found.", nameof(userId));
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email is required.", nameof(email));

            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
                throw new ArgumentException("User not found.", nameof(email));
            return user;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            if (string.IsNullOrEmpty(role) || (role != "Admin" && role != "Teacher" && role != "Parent"))
                throw new ArgumentException("Role must be 'Admin', 'Teacher', or 'Parent'.", nameof(role));

            return await _unitOfWork.Users.GetByRoleAsync(role);
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return false;

            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}