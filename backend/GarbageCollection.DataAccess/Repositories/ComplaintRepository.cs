using GarbageCollection.Common.Enums;
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
        public async Task<IReadOnlyList<Complaint>> GetComplaintsAsync(
            ComplaintStatus status,
            int page,
            int limit,
            CancellationToken ct = default)
        {
            var offset = (page - 1) * limit;

            return await _context.Complaints
                            .AsNoTracking()
                            .Where(c => c.Status == status)
                            .OrderBy(x => x.RequestAt)
                            .Skip(offset)
                            .Take(limit)
                            .ToListAsync(ct);
        }
        public Task<int> CountAsync(
         ComplaintStatus status,
         CancellationToken ct = default)
         => _context.Complaints
               .AsNoTracking()
               .CountAsync(c => c.Status == status, ct);

        public async Task<Complaint?> GetDetailAsync(
           Guid complaintId,
           CancellationToken ct = default)
        {
            return await _context.Complaints
                .AsNoTracking()

                // l?y ng??i t?o complaint
                .Include(c => c.Citizen)

                // l?y report + citizen c?a report
                .Include(c => c.Report)
                    .ThenInclude(r => r.Citizen)

                .FirstOrDefaultAsync(c => c.Id == Guid.Parse(complaintId.ToString()), ct);
        }
        public async Task ResolveAsync(
          Guid complaintId,
          string adminResponse,
          
          Guid adminId,
          DateTime resolvedAt,
          CancellationToken ct = default)
        {
            await _context.Complaints
                     .Where(c => c.Id == complaintId)
                     .ExecuteUpdateAsync(s => s
                         .SetProperty(c => c.Status, ComplaintStatus.Approved)
                         .SetProperty(c => c.AdminResponse, adminResponse)
                         .SetProperty(c => c.ResponseAt, resolvedAt),
                     ct);
        }
        public async Task<Complaint?> GetByIdAsync(Guid complaintId, CancellationToken ct = default)
        {
            return await _context.Complaints
                .AsNoTracking()
                .Include(c => c.Citizen)
                .Include(c => c.Report)
                    .ThenInclude(r => r.Citizen)
                .FirstOrDefaultAsync(c => c.Id == complaintId, ct);
        }
        public async Task<Complaint?> GetByIdWithReportAsync(Guid id, CancellationToken ct)
        {
            return await _context.Complaints
                .Include(c => c.Report)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

    
        public Task SaveChangesAsync(CancellationToken ct)
            => _context.SaveChangesAsync(ct);
    }
}
