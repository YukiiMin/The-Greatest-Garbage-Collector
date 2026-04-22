using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends an OTP verification email to the specified address.
        /// Implementations may use SMTP, SendGrid, SES, etc.
        /// </summary>
        Task SendOtpAsync(string toEmail, string otpCode, CancellationToken ct = default);
    }
}
