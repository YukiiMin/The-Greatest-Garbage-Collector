using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDbContext _context;

        public TeamRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Team?> GetByIdAsync(Guid id)
            => _context.Teams
                .Include(t => t.Collector)
                .FirstOrDefaultAsync(t => t.Id == id);

        public Task<IEnumerable<Team>> GetByCollectorIdAsync(Guid collectorId)
            => _context.Teams
                .Where(t => t.CollectorId == collectorId)
                .OrderBy(t => t.Name)
                .ToListAsync()
                .ContinueWith(r => (IEnumerable<Team>)r.Result);

        public async Task<IReadOnlyList<Team>> GetByCollectorIdsAsync(IEnumerable<Guid> collectorIds)
        {
            var ids = collectorIds.ToList();
            return await _context.Teams
                .Include(t => t.Collector)
                .Where(t => ids.Contains(t.CollectorId))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Team> CreateAsync(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<Team> UpdateAsync(Team team)
        {
            team.UpdatedAt = DateTime.UtcNow;
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task DeleteAsync(Team team)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }
    }
}
