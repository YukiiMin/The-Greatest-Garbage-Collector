using GarbageCollection.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Services
{

    /// <summary>
    /// Sends transactional emails via SMTP.
    /// Configure the "Email" section in appsettings.json (see README).
    /// Swap this implementation for SendGrid / SES by registering a different
    /// IEmailService in Program.cs — no other code changes required.
    /// </summary>
    public sealed class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IConfiguration configuration,
            ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpAsync(string toEmail, string otpCode, CancellationToken ct = default)
        {
            var section = _configuration.GetSection("Email");
            var host = section["SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost is missing.");
            var port = int.Parse(section["SmtpPort"] ?? "587");
            var user = section["SmtpUser"] ?? throw new InvalidOperationException("Email:SmtpUser is missing.");
            var password = section["SmtpPassword"] ?? throw new InvalidOperationException("Email:SmtpPassword is missing.");
            var fromAddr = section["FromAddress"] ?? user;
            var fromName = section["FromName"] ?? "GarbageCollection";

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10_000
            };

            var subject = "Your verification code";
            var body = BuildOtpEmailBody(otpCode);

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddr, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(message, ct);
                _logger.LogInformation("OTP email sent to {Email}.", toEmail);
            }
            catch (Exception ex)
            {
                // Log and rethrow — the service layer decides whether to surface this
                _logger.LogError(ex, "Failed to send OTP email to {Email}.", toEmail);
                throw;
            }
        }

        // ── Template ──────────────────────────────────────────────────────────

        private static string BuildOtpEmailBody(string otp) => $"""
            <html>
            <body style="font-family:Arial,sans-serif;color:#333;max-width:480px;margin:auto;padding:32px;">
              <h2 style="color:#2d7a4f;">Email Verification</h2>
              <p>Thank you for registering. Use the code below to verify your email address.
                 It expires in <strong>5 minutes</strong>.</p>
              <div style="font-size:36px;font-weight:bold;letter-spacing:8px;
                          color:#1a1a1a;padding:16px 0;">{otp}</div>
              <p style="color:#888;font-size:12px;">
                If you did not request this, please ignore this email.
              </p>
            </body>
            </html>
            """;
    }
}
