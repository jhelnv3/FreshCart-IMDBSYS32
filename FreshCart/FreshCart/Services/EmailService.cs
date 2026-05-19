using System;
using System.IO;

namespace FreshCart.Web.Services
{
    public class EmailService
    {
        private readonly string _logPath;

        public EmailService()
        {
            _logPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        public void SendPasswordResetEmail(string email, string resetCode)
        {
            try
            {
                var logFile = Path.Combine(_logPath, "email_log.txt");
                var logMessage = $@"
========================================
SIMULATED EMAIL
To: {email}
Subject: FreshCart - Password Reset Code
Body: 
Your password reset code is: {resetCode}
This code will expire in 15 minutes.
If you did not request this, please ignore this email.
========================================
Timestamp: {DateTime.Now}
";

                File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // Logging error - don't throw
            }
        }

        public void SendOrderConfirmation(string email, string orderNumber)
        {
            try
            {
                var logFile = Path.Combine(_logPath, "email_log.txt");
                var logMessage = $@"
========================================
SIMULATED EMAIL
To: {email}
Subject: FreshCart - Order Confirmation #{orderNumber}
Body: 
Your order #{orderNumber} has been placed successfully.
We will process your order shortly.
Thank you for shopping with FreshCart!
========================================
Timestamp: {DateTime.Now}
";

                File.AppendAllText(logFile, logMessage);
            }
            catch
            {
                // Logging error - don't throw
            }
        }
    }
}