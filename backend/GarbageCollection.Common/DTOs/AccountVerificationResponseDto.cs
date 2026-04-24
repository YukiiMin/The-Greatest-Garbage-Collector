using GarbageCollection.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    // ─── Response ────────────────────────────────────────────────────────────

    /// <summary>
    /// Payload returned inside ApiResponse on a successful auto-login verification.
    /// Fields match the spec exactly: email, full_name, avatar_url, address.
    /// Password hash and tokens are never included.
    /// </summary>
    public sealed class AccountVerificationResponseDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }
        public UserRole Role { get; set; }
    }
}
