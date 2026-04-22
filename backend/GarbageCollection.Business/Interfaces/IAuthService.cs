using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Auth.Local;
using GarbageCollection.Common.Models.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Validates a Google ID token, upserts the user, generates JWT pair.
        /// Returns a discriminated result that the controller maps to an HTTP response.
        /// </summary>
        Task<GoogleAuthResult> GoogleLoginAsync(string googleToken, CancellationToken ct = default);
        Task<LicenseResult> IssueLicenseAsync(string? rawRefreshTokenJwt, CancellationToken ct = default);
        Task RegisterAsync(LocalRegisterRequestDto data);
    }
}
