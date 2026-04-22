using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Helpers
{
    /// <summary>
    /// Generates cryptographically random, zero-padded 6-digit OTP codes.
    /// Using RandomNumberGenerator avoids the modulo bias of System.Random.
    /// </summary>
    public static class OtpHelper
    {
        private const int OtpLength = 6;
        private const int MaxValue = 1_000_000; // 10^6 → codes 000000 – 999999

        /// <summary>Returns a 6-digit string such as "047382".</summary>
        public static string Generate()
        {
            // GetInt32(0, maxValue) returns [0, maxValue)
            var value = RandomNumberGenerator.GetInt32(0, MaxValue);
            return value.ToString($"D{OtpLength}");
        }
    }
}
