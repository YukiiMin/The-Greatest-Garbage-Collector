using GarbageCollection.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    /// <summary>
    /// Validates an already-authenticated user's JWT claims against the database
    /// and returns their profile when everything checks out.
    ///
    /// Responsibilities (business layer only):
    ///   • Step 4 — compare JWT login_term against DB user.LoginTerm
    ///   • Step 5 — verify the user record still exists
    ///   • Step 6 — map user to AccountVerificationResponseDto
    ///
    /// Steps 1–3 (token extraction, signature, expiry) are handled upstream by
    /// ASP.NET Core's JWT middleware before the controller action is ever reached.
    /// The controller extracts the validated claims and passes them here.
    /// </summary>
    public interface IAccountVerificationService
    {
        /// <param name="email">Email claim extracted from the validated JWT.</param>
        /// <param name="tokenLoginTerm">login_term claim extracted from the validated JWT.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<AccountVerificationResult> VerifyAccountAsync(
            string email,
            int tokenLoginTerm,
            CancellationToken ct = default);
    }
}
