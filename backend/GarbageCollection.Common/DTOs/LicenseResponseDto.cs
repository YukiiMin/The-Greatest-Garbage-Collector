using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    // ─── Response ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Payload returned on a successful token refresh (license issuance).
    /// Fields: email, full_name, avatar_url, address — no token values in body.
    /// </summary>
    public sealed class LicenseResponseDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }
}
