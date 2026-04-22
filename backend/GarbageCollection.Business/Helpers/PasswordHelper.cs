using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Helpers
{
    /// <summary>
    /// Wraps BCrypt.Net-Next so the rest of the codebase never imports
    /// the hashing library directly (easy to swap algorithm later).
    /// Work factor 12 is a sensible production default (≈250 ms on modern hardware).
    /// </summary>
    public static class PasswordHelper
    {
        private const int WorkFactor = 12;

        /// <summary>Hashes a plain-text password using BCrypt.</summary>
        public static string Hash(string plainText)
            => BCrypt.Net.BCrypt.HashPassword(plainText, WorkFactor);

        /// <summary>
        /// Returns true when <paramref name="plainText"/> matches the stored
        /// <paramref name="hash"/>.
        /// </summary>
        public static bool Verify(string plainText, string hash)
            => BCrypt.Net.BCrypt.Verify(plainText, hash);
    }
}
