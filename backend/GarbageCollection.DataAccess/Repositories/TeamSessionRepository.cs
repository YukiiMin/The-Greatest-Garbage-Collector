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

        public Task<TeamSession?> GetActiveByTeamIdAsync(Guid teamId, DateOnly date)
            => _context.TeamSessions
                .Where(s => s.TeamId == teamId && s.Date == date && s.EndAt == null)
                .OrderByDescending(s => s.StartAt)
                .FirstOrDefaultAsync();

        public async Task<TeamSession> UpdateAsync(TeamSession session)
        {
            _context.TeamSessions.Update(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public Task<IEnumerable<TeamSession>> GetByTeamIdAsync(Guid teamId)
            => _context.TeamSessions
                .Where(s => s.TeamId == teamId)
                .OrderByDescending(s => s.StartAt)
                .ToListAsync()
                .ContinueWith(t => (IEnumerable<TeamSession>)t.Result);
    }
}
