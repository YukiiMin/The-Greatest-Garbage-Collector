using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Auth.Local
{
    public sealed class LocalRegisterResponseDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;
    }

    // ─── Internal service result ──────────────────────────────────────────────

    public sealed class RegisterResult
    {
        public bool Succeeded { get; init; }
        public int HttpStatusCode { get; init; }
        public string? FailMessage { get; init; }
        public string? FailCode { get; init; }
        public string? FailDescription { get; init; }
        public LocalRegisterResponseDto? Payload { get; init; }
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }

        public static RegisterResult Ok(
            LocalRegisterResponseDto payload,
            string accessToken,
            string refreshToken) => new()
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Payload = payload,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        public static RegisterResult Failure(
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
