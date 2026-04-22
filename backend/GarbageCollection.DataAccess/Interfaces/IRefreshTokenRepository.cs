using GarbageCollection.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken token, CancellationToken ct = default);
<<<<<<< HEAD

        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

        Task RevokeByIdAsync(Guid tokenId, CancellationToken ct = default);
=======
>>>>>>> 2b44a62e233f1c93c71d628b9c07ab83abfea1a0
        Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
