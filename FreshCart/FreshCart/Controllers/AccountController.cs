using FreshCart.Data;
using FreshCart.Web.Models.Entities;
using FreshCart.Web.Models.ViewModels;
using FreshCart.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace FreshCart.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(AuthService authService, ApplicationDbContext context, EmailService emailService)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                if (_authService.IsAuthenticated())
                    return RedirectToAction("Index", "Product");
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (_authService.Login(model.Username, model.Password))
                {
                    TempData["Success"] = "Welcome back!";
                    var role = HttpContext.Session.GetString("Role");
                    return role == "Admin" || role == "Staff"
                        ? RedirectToAction("Dashboard", "Admin")
                        : RedirectToAction("Index", "Product");
                }

                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Login failed: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_authService.IsAuthenticated())
                return RedirectToAction("Index", "Product");
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(model);
                }

                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = _authService.HashPassword(model.Password),
                    FullName = model.FullName,
                    Role = "Customer",
                    Address = "", // ADD THIS LINE
                    PhoneNumber = "",   // ADD                    
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Show the FULL error details
                var errorMessage = "Registration failed: " + ex.Message;

                // Dig into inner exceptions
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    errorMessage += " | Inner: " + innerException.Message;
                    innerException = innerException.InnerException;
                }

                ModelState.AddModelError("", errorMessage);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    TempData["Success"] = "If the email exists, a reset code will be displayed.";
                    return View();
                }

                // Generate 6-digit code
                var random = new Random();
                var code = random.Next(100000, 999999).ToString();

                // Save token
                var token = new PasswordResetToken
                {
                    UserId = user.UserId,
                    Token = code,
                    ExpiryDate = DateTime.Now.AddMinutes(15),
                    IsUsed = false
                };

                _context.PasswordResetTokens.Add(token);
                _context.SaveChanges();

                // Simulate sending email
                _emailService.SendPasswordResetEmail(user.Email, code);

                TempData["Success"] = $"A verification code has been sent. For demo purposes, your code is: {code}";
                TempData["ResetEmail"] = user.Email;

                return RedirectToAction("VerifyCode");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult VerifyCode()
        {
            // Check if we have email in TempData
            if (TempData["ResetEmail"] == null)
                return RedirectToAction("ForgotPassword");

            // Keep TempData alive for the next request
            TempData.Keep("ResetEmail");

            return View();
        }

        [HttpPost]
        public IActionResult VerifyCode(string code)
        {
            try
            {
                // Get email from TempData
                var email = TempData["ResetEmail"]?.ToString();
                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "Session expired. Please try again.";
                    return RedirectToAction("ForgotPassword");
                }

                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid request.");
                    TempData.Keep("ResetEmail");
                    return View();
                }

                var token = _context.PasswordResetTokens
                    .Where(t => t.UserId == user.UserId && !t.IsUsed && t.ExpiryDate > DateTime.Now)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault();

                if (token == null || token.Token != code)
                {
                    ModelState.AddModelError("", "Invalid or expired code.");
                    TempData.Keep("ResetEmail");
                    return View();
                }

                // Store verified email for reset password
                TempData["VerifiedEmail"] = email;
                TempData.Remove("ResetEmail");

                return RedirectToAction("ResetPassword");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Verification failed: " + ex.Message);
                return View();
            }
        }


        [HttpGet]
        public IActionResult ResetPassword()
        {
            // Check if we have verified email
            if (TempData["VerifiedEmail"] == null)
                return RedirectToAction("ForgotPassword");

            // Keep TempData alive
            TempData.Keep("VerifiedEmail");

            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData.Keep("VerifiedEmail");
                    return View(model);
                }

                var email = TempData["VerifiedEmail"]?.ToString();
                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "Session expired. Please try again.";
                    return RedirectToAction("ForgotPassword");
                }

                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    TempData.Keep("VerifiedEmail");
                    return View(model);
                }

                // Mark token as used
                var token = _context.PasswordResetTokens
                    .Where(t => t.UserId == user.UserId && !t.IsUsed && t.ExpiryDate > DateTime.Now)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault();

                if (token != null)
                {
                    token.IsUsed = true;
                }

                // Hash new password and save
                user.PasswordHash = _authService.HashPassword(model.Password);
                _context.Users.Update(user);  // ADD THIS - mark user as modified
                _context.SaveChanges();

                TempData.Remove("VerifiedEmail");
                TempData["Success"] = "Password reset successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Password reset failed: " + ex.Message);
                TempData.Keep("VerifiedEmail");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            _authService.Logout();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }        
    }
}