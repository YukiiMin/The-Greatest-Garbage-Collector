using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    // ─── Internal discriminated result ────────────────────────────────────────
    // Returned by IAuthService.IssueLicenseAsync.
    // The controller maps this to IActionResult — zero business logic leaks up.

    public sealed class LicenseResult
    {
        public bool Succeeded { get; init; }
        public int HttpStatusCode { get; init; }

        // Failure fields
        public string? FailMessage { get; init; }
        public string? FailCode { get; init; }
        public string? FailDescription { get; init; }

        // Success fields — tokens travel only via cookies, never in the body
        public LicenseResponseDto? Payload { get; init; }
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }

        // ── Factory helpers ───────────────────────────────────────────────────

        public static LicenseResult Ok(
            LicenseResponseDto payload,
            string accessToken,
            string refreshToken) => new()
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Payload = payload,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        public static LicenseResult Failure(
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
