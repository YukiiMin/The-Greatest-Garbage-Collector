using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Auth
{
    public sealed class LoginResult
    {
        public bool Succeeded { get; init; }
        public int HttpStatusCode { get; init; }

        // Failure fields
        public string? FailMessage { get; init; }
        public string? FailCode { get; init; }
        public string? FailDescription { get; init; }

        // Success fields
        public LocalLoginResponseDto? Payload { get; init; }
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }

        // ── factory helpers ───────────────────────────────────────────────────

        public static LoginResult Ok(
            LocalLoginResponseDto payload,
            string accessToken,
            string refreshToken) => new()
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Payload = payload,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        public static LoginResult Failure(
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
