using GarbageCollection.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    /// <summary>
    /// Handles the full OTP resend flow for email verification.
    ///
    /// JWT steps 1–3 (token extraction, signature, expiry) are handled upstream by
    /// ASP.NET Core's JWT middleware via [Authorize] on the controller action.
    /// The controller extracts the validated email claim and passes it here.
    ///
    /// This service is responsible for:
    ///   1. Validating the request email format
    ///   2. Comparing the request email with the token email (cross-account guard)
    ///   3. Checking the user exists in the database
    ///   4. Checking the user's email is not already verified
    ///   5. Applying rate limiting (time window + max-attempt count)
    ///   6. Generating, hashing and persisting a new OTP (create or update)
    ///   7. Sending the plain-text OTP via IEmailService
    /// </summary>
    public interface IResendOtpService
    {
        /// <param name="request">Request DTO containing the email field.</param>
        /// <param name="tokenEmail">Email extracted from the validated JWT by the controller.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<ResendOtpResult> ResendOtpAsync(
            ResendOtpRequestDto request,
            string tokenEmail,
            CancellationToken ct = default);
    }
}
