using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    public sealed class ResendOtpResult
    {
        public bool Succeeded { get; init; }
        public int HttpStatusCode { get; init; }

        // Failure fields
        public string? FailMessage { get; init; }
        public string? FailCode { get; init; }
        public string? FailDescription { get; init; }

        // Success fields
        public ResendOtpResponseDto? Payload { get; init; }

        // ── Factory helpers ───────────────────────────────────────────────────

        public static ResendOtpResult Ok(ResendOtpResponseDto payload) => new()
        {
            Succeeded = true,
            HttpStatusCode = 200,
            Payload = payload
        };

        public static ResendOtpResult Failure(
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
