using GarbageCollection.Common.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.Models.Internal
{
    public sealed class GoogleAuthResult
    {
        public bool Succeeded { get; init; }
        public int HttpStatusCode { get; init; }

        public string? FailMessage { get; init; }
        public string? FailCode { get; init; }
        public string? FailDescription { get; init; }

        public GoogleLoginResponseDto? Payload { get; init; }

        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }

        // SUCCESS
        public static GoogleAuthResult Ok(
            GoogleLoginResponseDto payload,
            string access,
            string refresh) => new()
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Payload = payload,
                AccessToken = access,
                RefreshToken = refresh
            };

        // FAILURE
        public static GoogleAuthResult Failure(
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
