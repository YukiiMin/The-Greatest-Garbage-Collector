using GarbageCollection.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    public interface IVerifyEmailService
    {
        /// <summary>
        /// Validates the OTP submitted by the user and marks their email as verified.
        /// The tokenEmail parameter is the email extracted from the JWT by the controller —
        /// the service compares it against the request email to prevent cross-account attacks.
        /// </summary>
        Task<VerifyEmailResult> VerifyEmailAsync(
            VerifyEmailRequestDto request,
            string tokenEmail,
            CancellationToken ct = default);
    }
}
