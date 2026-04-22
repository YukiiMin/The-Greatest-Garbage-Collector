using GarbageCollection.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Interfaces
{
    public interface IPasswordOtpService
    {
        Task<PasswordOtpResult> CreatePasswordOtpAsync(
            CreatePasswordOtpRequestDto request,
            CancellationToken ct = default);
    }
}
