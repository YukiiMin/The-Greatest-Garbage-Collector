using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Helpers
{
    /// <summary>
    /// Stateless validation helpers for email and password rules.
    /// All regexes are compiled once at startup for maximum throughput.
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
            RegexOptions.IgnoreCase);

        private static readonly Regex PasswordRegex = new(
            @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()\-_=+\[\]{}|;':"",./<>?`~\\]).{8,16}$");

        public static bool IsValidEmail(string? email)
            => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);

        public static bool IsValidPassword(string? password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            if (password.Any(char.IsWhiteSpace)) return false;
            return PasswordRegex.IsMatch(password);
        }

        public static string? GetPasswordValidationError(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return "Password must not be empty.";
            if (password.Any(char.IsWhiteSpace))
                return "Password must not contain whitespace.";
            if (password.Length < 8 || password.Length > 16)
                return "Password must be between 8 and 16 characters.";
            if (!password.Any(char.IsUpper))
                return "Password must contain at least one uppercase letter.";
            if (!password.Any(char.IsLower))
                return "Password must contain at least one lowercase letter.";
            if (!password.Any(char.IsDigit))
                return "Password must contain at least one digit.";
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return "Password must contain at least one special character.";
            return null;
        }
    }
}
