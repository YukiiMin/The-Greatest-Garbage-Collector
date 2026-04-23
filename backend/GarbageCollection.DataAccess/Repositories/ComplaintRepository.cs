using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

namespace GarbageCollection.DataAccess.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly AppDbContext _context;

        public ComplaintRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Complaint> CreateAsync(Complaint complaint)
        {
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task<Complaint?> GetByIdAsync(Guid id)
            => await _context.Complaints
                .Include(c => c.Citizen)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Complaint> UpdateAsync(Complaint complaint)
        {
            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task<(IEnumerable<Complaint> Items, int Total)> GetByReportIdPagedAsync(Guid reportId, int page, int limit)
        {
            var query = _context.Complaints
                .Where(c => c.ReportId == reportId)
                .OrderByDescending(c => c.RequestAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (items, total);
        }

        public async Task AppendMessageAsync(Guid complaintId, ComplaintMessage message, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(new[] { message });
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE complaints SET messages = COALESCE(messages, '[]'::jsonb) || @msg::jsonb WHERE id = @id",
                new NpgsqlParameter("@msg", json),
                new NpgsqlParameter("@id", complaintId));
        }
    }
}
