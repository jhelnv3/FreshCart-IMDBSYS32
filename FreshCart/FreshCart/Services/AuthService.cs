using FreshCart.Data;
using FreshCart.Web.Models.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace FreshCart.Web.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                if (string.IsNullOrEmpty(hash))
                {
                    System.Diagnostics.Debug.WriteLine("VerifyPassword: Hash is null or empty");
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    System.Diagnostics.Debug.WriteLine("VerifyPassword: Password is null or empty");
                    return false;
                }

                bool result = BCrypt.Net.BCrypt.Verify(password, hash);
                System.Diagnostics.Debug.WriteLine($"VerifyPassword: Result = {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VerifyPassword Error: {ex.Message}");
                return false;
            }
        }

        public bool Login(string username, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Login attempt: Username='{username}'");

                var user = _context.Users.FirstOrDefault(u =>
                    u.Username == username && u.IsActive);

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Login failed: User '{username}' not found");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"User found: {user.UserId}, Hash length: {user.PasswordHash?.Length}");

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    System.Diagnostics.Debug.WriteLine($"Login failed: Invalid password for '{username}'");
                    return false;
                }

                var session = _httpContextAccessor.HttpContext.Session;
                session.SetString("UserId", user.UserId.ToString());
                session.SetString("Username", user.Username);
                session.SetString("Role", user.Role);
                session.SetString("FullName", user.FullName);

                System.Diagnostics.Debug.WriteLine($"Login successful: {username}, Role: {user.Role}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Login error stack: {ex.StackTrace}");
                return false;
            }
        }

        public void Logout()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.Clear();
        }

        public User GetCurrentUser()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext.Session;
                var userIdString = session.GetString("UserId");

                if (string.IsNullOrEmpty(userIdString))
                    return null;

                int userId = int.Parse(userIdString);
                return _context.Users.Find(userId);
            }
            catch
            {
                return null;
            }
        }

        public bool IsAuthenticated()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            return !string.IsNullOrEmpty(session.GetString("UserId"));
        }

        public bool IsInRole(string role)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            return session.GetString("Role") == role;
        }

        public bool IsInAnyRole(params string[] roles)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var userRole = session.GetString("Role");
            return roles.Contains(userRole);
        }
    }
}