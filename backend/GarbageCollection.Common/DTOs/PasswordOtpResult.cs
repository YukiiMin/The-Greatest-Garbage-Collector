using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    // ─── Internal discriminated result ────────────────────────────────────────
    // Returned by IPasswordOtpService.CreatePasswordOtpAsync.
    // The controller maps it to IActionResult — zero business logic leaks up.

    public sealed class PasswordOtpResult
    {
        public bool Succeeded { get; init; }
        public int HttpStatusCode { get; init; }

        // Failure fields
        public string? FailMessage { get; init; }
        public string? FailCode { get; init; }
        public string? FailDescription { get; init; }

        // Success fields
        public CreatePasswordOtpResponseDto? Payload { get; init; }

        // ── Factory helpers ───────────────────────────────────────────────────

        public static PasswordOtpResult Ok(CreatePasswordOtpResponseDto payload) => new()
        {
            Succeeded = true,
            HttpStatusCode = 200,
            Payload = payload
        };

        public static PasswordOtpResult Failure(
            int statusCode,
            string message,
            string code,
            string description) => new()
            {
                Succeeded = false,
                HttpStatusCode = statusCode,
                FailMessage = message,
                FailCode = code,
                FailDescription = description
            };
    }
}
