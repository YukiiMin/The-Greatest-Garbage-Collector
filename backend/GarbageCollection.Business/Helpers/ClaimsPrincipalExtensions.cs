using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns the email from the JWT (ClaimTypes.Email).
        /// Returns null if the claim is absent or empty — the caller decides the error response.
        /// </summary>
        public static string? GetEmail(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.Email);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>Returns the full_name claim set during token generation.</summary>
        public static string? GetFullName(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue("full_name");
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>Returns the login_term claim as an int (defaults to 0 if absent).</summary>
        public static int GetLoginTerm(this ClaimsPrincipal principal)
        {
            var raw = principal.FindFirstValue("login_term");
            return int.TryParse(raw, out var term) ? term : 0;
        }
    }
}
