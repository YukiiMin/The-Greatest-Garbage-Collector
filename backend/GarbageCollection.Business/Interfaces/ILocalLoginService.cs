using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    public interface ILocalLoginService
    {
        /// <summary>
        /// Authenticates a local (email + password) user.
        /// Returns a discriminated LoginResult the controller maps to an HTTP response.
        /// All business logic — validation, credential check, token generation — lives here.
        /// </summary>
        Task<LoginResult> LoginAsync(
            LocalLoginRequestDto request,
            CancellationToken ct = default);
    }
}
