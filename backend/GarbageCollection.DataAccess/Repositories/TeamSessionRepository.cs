using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class TeamSessionRepository : ITeamSessionRepository
    {
        private readonly AppDbContext _context;

        public TeamSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TeamSession> CreateAsync(TeamSession session)
        {
            _context.TeamSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public Task<TeamSession?> GetByTeamAndDateAsync(Guid teamId, DateOnly date)
            => _context.TeamSessions
                .FirstOrDefaultAsync(s => s.TeamId == teamId && s.Date == date);

        public async Task UpdateAsync(TeamSession session)
        {
            _context.TeamSessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<TeamSession>> GetByTeamSinceAsync(Guid teamId, DateTime since)
            => await _context.TeamSessions
                .AsNoTracking()
                .Where(s => s.TeamId == teamId && s.StartAt >= since)
                .OrderByDescending(s => s.StartAt)
                .ToListAsync();

        public async Task<IReadOnlyList<TeamSession>> GetByTeamIdsAsync(
            IEnumerable<Guid> teamIds, CancellationToken ct = default)
        {
            var list = teamIds.ToList();
            return await _context.TeamSessions
                .AsNoTracking()
                .Where(s => list.Contains(s.TeamId))
                .ToListAsync(ct);
        }
    }
}
