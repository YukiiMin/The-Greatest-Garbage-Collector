using GarbageCollection.Common.DTOs.Auth.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    public interface ILocalAuthService
    {
        /// <summary>
        /// Validates input, creates the user, generates OTP, issues JWT pair.
        /// Returns a discriminated result the controller maps to an HTTP response.
        /// </summary>
        Task<RegisterResult> RegisterAsync(
            LocalRegisterRequestDto request,
            CancellationToken ct = default);
    }
}
