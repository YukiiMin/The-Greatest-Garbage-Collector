using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GarbageCollection.Common.Models;

namespace GarbageCollection.Common.DTOs
{
    /// <summary>
    /// Represents a one-time password record for the password-reset flow.
    ///
    /// Design notes:
    ///   • Email is the primary key (mirrors the DB schema: email VARCHAR PRIMARY KEY).
    ///     Unlike EmailOtp (which uses a Guid PK), there is exactly one row per email.
    ///   • OtpCode stores a BCrypt hash of the plain-text OTP — the raw value is
    ///     only ever sent by email and never persisted.
    ///   • Count tracks how many times an OTP has been (re-)generated for this email.
    ///     Incremented on every upsert.
    ///   • LastUpdatedAt is set on every update; it is the reference point used to
    ///     check whether the OTP window (5 minutes) has elapsed.
    /// </summary>
    public class PasswordOtp
    {
        /// <summary>Primary key — the user's email address.</summary>
        public string Email { get; set; } = null!;

        /// <summary>BCrypt hash of the raw OTP code. Never store plain-text.</summary>
        public string OtpCode { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

        /// <summary>Number of OTPs generated for this email (incremented on upsert).</summary>
        public int Count { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp of the most recent OTP generation.
        /// Set on initial insert and updated on every subsequent resend.
        /// </summary>
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ───────────────────────────────────────────────────────
        [ForeignKey(nameof(Email))]
        public GarbageCollection.Common.Models.User User { get; set; } = null!;
    }
}
