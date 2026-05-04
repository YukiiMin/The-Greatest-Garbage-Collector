using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Data
{
    /// <summary>
    /// Seed dữ liệu test.
    ///
    /// ════════════════════════════════════════════════════════════════
    ///  TÀI KHOẢN TEST (plaintext passwords)
    /// ════════════════════════════════════════════════════════════════
    ///  Email                              Password               Role
    ///  ─────────────────────────────────  ─────────────────────  ──────────
    ///  admin@ecoconnect.vn                Admin@123456           Admin
    ///  enterprise@ecoconnect.vn           Enterprise@123456      Enterprise
    ///  enterprise2@ecoconnect.vn          Enterprise@123456      Enterprise
    ///  enterprise3@ecoconnect.vn          Enterprise@123456      Enterprise
    ///  collectorstaff1@ecoconnect.vn      Collectorstaff@123456  Collector
    ///  collectorstaff2@ecoconnect.vn      Collectorstaff@123456  Collector
    ///  collectorstaff3@ecoconnect.vn      Collectorstaff@123456  Collector
    ///  citizen1@ecoconnect.vn             Citizen@123456         Citizen
    ///  citizen2@ecoconnect.vn             Citizen@123456         Citizen
    ///  (+ 8 more citizen accounts up to citizen10)
    /// ════════════════════════════════════════════════════════════════
    /// </summary>
    public static class DbSeeder
    {
        private const string HashAdmin           = "$2a$12$aHISWOyPFiALHM4ui0SHuup.QxXMXw/BH7apQ1S87rgi6OjdRWZGK"; // Admin@123456
        private const string HashEnterprise      = "$2a$12$TKDLYwSJEhcw/7alnx9D4eGdh.ZbOvieyDAL1Rjdf14n4EScaj/sW"; // Enterprise@123456
        private const string HashCollectorStaff  = "$2a$12$Nvjf5anVHyOQlHfFLBYMC.jLxqhm1L2VPvPWnFvPuC7c0siwHijW2"; // Collectorstaff@123456
        private const string HashCitizen         = "$2a$12$Tab8V.c1354AApY.rW7fhOtj2mpFxGEQ40dKygtOopDx8BTFmGMyq"; // Citizen@123456

        // ── Public entry points ───────────────────────────────────────────────

        /// <summary>Seed lần đầu — chỉ chạy nếu bảng user còn trống.</summary>
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Users.AnyAsync()) return;
            await DoSeedAsync(db);
        }

        /// <summary>Xóa toàn bộ dữ liệu và seed lại từ đầu.</summary>
        public static async Task ReseedAsync(AppDbContext db)
        {
            await ClearAllAsync(db);
            await DoSeedAsync(db);
        }

        // ── Clear ─────────────────────────────────────────────────────────────

        private static async Task ClearAllAsync(AppDbContext db)
        {
            // Xóa theo thứ tự FK-safe
            db.Complaints.RemoveRange(await db.Complaints.ToListAsync());
            await db.SaveChangesAsync();

            db.CitizenReports.RemoveRange(await db.CitizenReports.ToListAsync());
            await db.SaveChangesAsync();

            db.TeamSessions.RemoveRange(await db.TeamSessions.ToListAsync());
            await db.SaveChangesAsync();

            db.CollectorStaffs.RemoveRange(await db.CollectorStaffs.ToListAsync());
            await db.SaveChangesAsync();

            db.EnterpriseStaffs.RemoveRange(await db.EnterpriseStaffs.ToListAsync());
            await db.SaveChangesAsync();

            db.Teams.RemoveRange(await db.Teams.ToListAsync());
            await db.SaveChangesAsync();

            db.CollectorHubs.RemoveRange(await db.CollectorHubs.ToListAsync());
            await db.SaveChangesAsync();

            db.Collectors.RemoveRange(await db.Collectors.ToListAsync());
            await db.SaveChangesAsync();

            db.PointCategories.RemoveRange(await db.PointCategories.ToListAsync());
            await db.SaveChangesAsync();

            db.Enterprises.RemoveRange(await db.Enterprises.ToListAsync());
            await db.SaveChangesAsync();

            db.RefreshTokens.RemoveRange(await db.RefreshTokens.ToListAsync());
            await db.SaveChangesAsync();

            db.Users.RemoveRange(await db.Users.ToListAsync());
            await db.SaveChangesAsync();

            db.WorkAreas.RemoveRange(await db.WorkAreas.ToListAsync());
            await db.SaveChangesAsync();
        }

        // ── Core seed ─────────────────────────────────────────────────────────

        private static async Task DoSeedAsync(AppDbContext db)
        {
            var wa  = await SeedWorkAreasAsync(db);
            var usr = await SeedUsersAsync(db, wa);
            var org = await SeedEnterprisesAsync(db, wa, usr);
            var col = await SeedCollectorOrgsAsync(db, wa, usr, org);
            await SeedCitizenReportsAsync(db, wa, usr, org, col);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 1. WorkAreas — 2 quận, mỗi quận 4 phường
        // ─────────────────────────────────────────────────────────────────────
        private static async Task<WorkAreaIds> SeedWorkAreasAsync(AppDbContext db)
        {
            var dq3 = Guid.NewGuid(); var dq1 = Guid.NewGuid();
            var wp1 = Guid.NewGuid(); var wp2 = Guid.NewGuid();
            var wp3 = Guid.NewGuid(); var wp4 = Guid.NewGuid();
            var wq1p1 = Guid.NewGuid(); var wq1p2 = Guid.NewGuid();

            db.WorkAreas.AddRange(
                new WorkArea { Id = dq3,  Name = "Quận 3",    Type = "District", ParentId = null, CreatedAt = DateTime.UtcNow },
                new WorkArea { Id = dq1,  Name = "Quận 1",    Type = "District", ParentId = null, CreatedAt = DateTime.UtcNow },
                new WorkArea { Id = wp1,  Name = "Phường 1",  Type = "Ward", ParentId = dq3, CreatedAt = DateTime.UtcNow },
                new WorkArea { Id = wp2,  Name = "Phường 2",  Type = "Ward", ParentId = dq3, CreatedAt = DateTime.UtcNow },
                new WorkArea { Id = wp3,  Name = "Phường 3",  Type = "Ward", ParentId = dq3, CreatedAt = DateTime.UtcNow },
                new WorkArea { Id = wp4,  Name = "Phường 4",  Type = "Ward", ParentId = dq3, CreatedAt = DateTime.UtcNow },
                new WorkArea { Id = wq1p1, Name = "Phường Bến Nghé", Type = "Ward", ParentId = dq1, CreatedAt = DateTime.UtcNow },
                new WorkArea { Id = wq1p2, Name = "Phường Bến Thành", Type = "Ward", ParentId = dq1, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            return new WorkAreaIds
            {
                DistrictQ3Id = dq3, DistrictQ1Id = dq1,
                WardP1Id = wp1, WardP2Id = wp2, WardP3Id = wp3, WardP4Id = wp4,
                WardQ1P1Id = wq1p1
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. Users — 1 admin, 3 enterprise, 3 collector staff, 10 citizen
        // ─────────────────────────────────────────────────────────────────────
        private static async Task<UserIds> SeedUsersAsync(AppDbContext db, WorkAreaIds wa)
        {
            var now = DateTime.UtcNow;

            User U(string email, string name, string hash, UserRole role, Guid? ward, string? addr = null, int points = 0) => new()
            {
                Id = Guid.NewGuid(), Email = email, EmailVerified = true, Provider = "local",
                FullName = name, PasswordHash = hash, Role = role,
                WorkAreaId = ward, Address = addr, TotalPoints = points, CreatedAt = now, UpdatedAt = now
            };

            var admin  = U("admin@ecoconnect.vn",           "Admin EcoConnect",          HashAdmin,          UserRole.Admin,      null);
            var ent1   = U("enterprise@ecoconnect.vn",      "Nguyễn Văn Quản Lý",        HashEnterprise,     UserRole.Enterprise, wa.DistrictQ3Id);
            var ent2   = U("enterprise2@ecoconnect.vn",     "Trần Thị Điều Phối",         HashEnterprise,     UserRole.Enterprise, wa.DistrictQ3Id);
            var ent3   = U("enterprise3@ecoconnect.vn",     "Lê Minh Giám Sát",           HashEnterprise,     UserRole.Enterprise, wa.DistrictQ3Id);
            var col1   = U("collectorstaff1@ecoconnect.vn", "Phạm Thị Thu Gom",           HashCollectorStaff, UserRole.Collector,  wa.WardP1Id);
            var col2   = U("collectorstaff2@ecoconnect.vn", "Ngô Văn Tài Xế",             HashCollectorStaff, UserRole.Collector,  wa.WardP1Id);
            var col3   = U("collectorstaff3@ecoconnect.vn", "Đặng Quốc Nhân Viên",        HashCollectorStaff, UserRole.Collector,  wa.WardP2Id);
            int[] totals  = { 1500, 800, 620, 430, 950, 270, 1100, 340, 720, 580 };
            var cit1   = U("citizen1@ecoconnect.vn",        "Lê Văn Công Dân",            HashCitizen,        UserRole.Citizen,    wa.WardP1Id, "123 Lê Văn Sỹ, P1, Q3",             totals[0]);
            var cit2   = U("citizen2@ecoconnect.vn",        "Phạm Thị Dân",               HashCitizen,        UserRole.Citizen,    wa.WardP2Id, "456 Nam Kỳ Khởi Nghĩa, P2, Q3",     totals[1]);
            var cit3   = U("citizen3@ecoconnect.vn",        "Ngô Thị Bình",               HashCitizen,        UserRole.Citizen,    wa.WardP1Id, "78 Trần Quốc Thảo, P1, Q3",          totals[2]);
            var cit4   = U("citizen4@ecoconnect.vn",        "Đinh Văn Hùng",              HashCitizen,        UserRole.Citizen,    wa.WardP2Id, "12 Cao Thắng, P2, Q3",               totals[3]);
            var cit5   = U("citizen5@ecoconnect.vn",        "Vũ Thị Lan",                 HashCitizen,        UserRole.Citizen,    wa.WardP3Id, "99 Nguyễn Thiện Thuật, P3, Q3",      totals[4]);
            var cit6   = U("citizen6@ecoconnect.vn",        "Hoàng Văn Nam",              HashCitizen,        UserRole.Citizen,    wa.WardP3Id, "34 Bà Huyện Thanh Quan, P3, Q3",     totals[5]);
            var cit7   = U("citizen7@ecoconnect.vn",        "Đặng Thị Hoa",               HashCitizen,        UserRole.Citizen,    wa.WardP4Id, "55 Võ Văn Tần, P4, Q3",              totals[6]);
            var cit8   = U("citizen8@ecoconnect.vn",        "Trương Quốc Bảo",            HashCitizen,        UserRole.Citizen,    wa.WardP4Id, "11 Lý Chính Thắng, P4, Q3",          totals[7]);
            var cit9   = U("citizen9@ecoconnect.vn",        "Bùi Thị Thanh",              HashCitizen,        UserRole.Citizen,    wa.WardP1Id, "200 Đinh Tiên Hoàng, P1, Q3",        totals[8]);
            var cit10  = U("citizen10@ecoconnect.vn",       "Lâm Văn Phúc",               HashCitizen,        UserRole.Citizen,    wa.WardP2Id, "88 Trần Huy Liệu, P2, Q3",           totals[9]);

            var citizens = new[] { cit1, cit2, cit3, cit4, cit5, cit6, cit7, cit8, cit9, cit10 };
            db.Users.AddRange(admin, ent1, ent2, ent3, col1, col2, col3,
                              cit1, cit2, cit3, cit4, cit5,
                              cit6, cit7, cit8, cit9, cit10);
            await db.SaveChangesAsync();

            return new UserIds
            {
                AdminId  = admin.Id,
                Ent1Id   = ent1.Id,
                Ent2Id   = ent2.Id,
                Ent3Id   = ent3.Id,
                Col1Id   = col1.Id,
                Col2Id   = col2.Id,
                Col3Id   = col3.Id,
                Cit      = citizens.Select(c => c.Id).ToArray()
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. Enterprise + EnterpriseStaff (3) + PointCategory
        // ─────────────────────────────────────────────────────────────────────
        private static async Task<OrgIds> SeedEnterprisesAsync(AppDbContext db, WorkAreaIds wa, UserIds usr)
        {
            var now = DateTime.UtcNow;

            var enterprise = new Enterprise
            {
                Id = Guid.NewGuid(), Name = "Công ty Môi trường Đô thị Quận 3",
                PhoneNumber = "02838386666", Email = "contact@mtdt-q3.vn",
                Address = "1 Võ Thị Sáu, Quận 3, TP.HCM",
                Latitude = 10.7877m, Longitude = 106.6830m,
                WorkAreaId = wa.DistrictQ3Id, CreatedAt = now, UpdatedAt = now
            };
            db.Enterprises.Add(enterprise);
            await db.SaveChangesAsync();

            // 3 enterprise staff thuộc cùng 1 enterprise
            db.EnterpriseStaffs.AddRange(
                new EnterpriseStaff { UserId = usr.Ent1Id, EnterpriseId = enterprise.Id, JoinHubAt = now },
                new EnterpriseStaff { UserId = usr.Ent2Id, EnterpriseId = enterprise.Id, JoinHubAt = now },
                new EnterpriseStaff { UserId = usr.Ent3Id, EnterpriseId = enterprise.Id, JoinHubAt = now }
            );

            db.PointCategories.Add(new PointCategory
            {
                Id = Guid.NewGuid(), Name = "Báo cáo rác hợp lệ - Mặc định",
                EnterpriseId = enterprise.Id, IsActive = true,
                Mechanic = new PointMechanic
                {
                    Organic       = new WasteTypeCriteria { Points = 10, MinWeightGrams = 500 },
                    Recyclable    = new WasteTypeCriteria { Points = 15, MinWeightGrams = 300 },
                    NonRecyclable = new WasteTypeCriteria { Points = 5,  MinWeightGrams = 1000 },
                },
                CreatedAt = now, UpdatedAt = now
            });
            await db.SaveChangesAsync();

            return new OrgIds { EnterpriseId = enterprise.Id };
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. Collector org + 2 CollectorHub + 3 Team + 3 CollectorStaff
        // ─────────────────────────────────────────────────────────────────────
        private static async Task<ColIds> SeedCollectorOrgsAsync(AppDbContext db, WorkAreaIds wa, UserIds usr, OrgIds org)
        {
            var now = DateTime.UtcNow;

            // Collector org 1 — Phường 1
            var col1 = new Collector
            {
                Id = Guid.NewGuid(), Name = "Đội thu gom Phường 1 - Quận 3",
                PhoneNumber = "02838381111", Email = "collector-p1@mtdt-q3.vn",
                Address = "10 Nguyễn Thiện Thuật, P1, Q3",
                Latitude = 10.7800m, Longitude = 106.6800m,
                WorkAreaId = wa.WardP1Id, EnterpriseId = org.EnterpriseId,
                CreatedAt = now, UpdatedAt = now
            };
            // Collector org 2 — Phường 2
            var col2 = new Collector
            {
                Id = Guid.NewGuid(), Name = "Đội thu gom Phường 2 - Quận 3",
                PhoneNumber = "02838382222", Email = "collector-p2@mtdt-q3.vn",
                Address = "5 Nam Kỳ Khởi Nghĩa, P2, Q3",
                Latitude = 10.7820m, Longitude = 106.6820m,
                WorkAreaId = wa.WardP2Id, EnterpriseId = org.EnterpriseId,
                CreatedAt = now, UpdatedAt = now
            };
            db.Collectors.AddRange(col1, col2);
            await db.SaveChangesAsync();

            // CollectorHub 1
            var hub1 = new CollectorHub
            {
                Id = Guid.NewGuid(), Name = "Garage thu gom P1",
                PhoneNumber = "02838381100", Email = "hub1@mtdt-q3.vn",
                Address = "12 Nguyễn Thiện Thuật, P1, Q3",
                Latitude = 10.7802m, Longitude = 106.6802m,
                WorkAreaId = wa.WardP1Id, AssignedCapacity = 200,
                CreatedAt = now, UpdatedAt = now
            };
            // CollectorHub 2
            var hub2 = new CollectorHub
            {
                Id = Guid.NewGuid(), Name = "Garage thu gom P2",
                PhoneNumber = "02838382200", Email = "hub2@mtdt-q3.vn",
                Address = "8 Cao Thắng, P2, Q3",
                Latitude = 10.7822m, Longitude = 106.6822m,
                WorkAreaId = wa.WardP2Id, AssignedCapacity = 150,
                CreatedAt = now, UpdatedAt = now
            };
            db.CollectorHubs.AddRange(hub1, hub2);
            await db.SaveChangesAsync();

            // Team Alpha — hub1, P1
            var teamA = new Team
            {
                Id = Guid.NewGuid(), Name = "Team Alpha",
                TotalCapacity = 80m, IsActive = true,
                CollectorHubId = hub1.Id, WorkAreaId = wa.WardP1Id,
                DispatchTime = "06:00", RouteOptimized = false, InWork = false,
                CreatedAt = now, UpdatedAt = now
            };
            // Team Beta — hub1, P1
            var teamB = new Team
            {
                Id = Guid.NewGuid(), Name = "Team Beta",
                TotalCapacity = 60m, IsActive = true,
                CollectorHubId = hub1.Id, WorkAreaId = wa.WardP1Id,
                DispatchTime = "14:00", RouteOptimized = false, InWork = false,
                CreatedAt = now, UpdatedAt = now
            };
            // Team Gamma — hub2, P2
            var teamG = new Team
            {
                Id = Guid.NewGuid(), Name = "Team Gamma",
                TotalCapacity = 70m, IsActive = true,
                CollectorHubId = hub2.Id, WorkAreaId = wa.WardP2Id,
                DispatchTime = "07:00", RouteOptimized = false, InWork = false,
                CreatedAt = now, UpdatedAt = now
            };
            db.Teams.AddRange(teamA, teamB, teamG);
            await db.SaveChangesAsync();

            // CollectorStaffs
            db.CollectorStaffs.AddRange(
                new CollectorStaff { UserId = usr.Col1Id, CollectorId = col1.Id, CollectorHubId = hub1.Id, TeamId = teamA.Id, JoinTeamAt = now.AddDays(-60) },
                new CollectorStaff { UserId = usr.Col2Id, CollectorId = col1.Id, CollectorHubId = hub1.Id, TeamId = teamB.Id, JoinTeamAt = now.AddDays(-45) },
                new CollectorStaff { UserId = usr.Col3Id, CollectorId = col2.Id, CollectorHubId = hub2.Id, TeamId = teamG.Id, JoinTeamAt = now.AddDays(-30) }
            );
            await db.SaveChangesAsync();

            return new ColIds
            {
                Col1Id = col1.Id, Col2Id = col2.Id,
                Hub1Id = hub1.Id, Hub2Id = hub2.Id,
                TeamAId = teamA.Id, TeamBId = teamB.Id, TeamGId = teamG.Id
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // 5. CitizenReports (~50) + Complaints (10)
        // ─────────────────────────────────────────────────────────────────────
        private static async Task SeedCitizenReportsAsync(
            AppDbContext db, WorkAreaIds wa, UserIds usr, OrgIds org, ColIds col)
        {
            var now     = DateTime.UtcNow;
            var reports = new List<CitizenReport>();

            // Helper tạo report
            CitizenReport R(
                Guid citizenId, ReportStatus status, int daysAgo,
                Guid? teamId = null, Guid? assignBy = null,
                int? pointVal = null, string? note = null,
                List<WasteType>? types = null, decimal? capacity = null)
            {
                var reportAt = now.AddDays(-daysAgo).AddHours(-new Random(daysAgo + citizenId.GetHashCode()).Next(0, 18));
                DateTime? assignAt  = null, deadline = null, startAt = null, collectedAt = null, completeAt = null, updatedAt = null;

                if (status >= ReportStatus.Queue)    { assignAt = reportAt.AddHours(2); deadline = assignAt.Value.AddDays(1); }
                if (status >= ReportStatus.Assigned) { assignAt = reportAt.AddHours(2); deadline = assignAt.Value.AddDays(1); }
                if (status >= ReportStatus.Processing) { startAt = assignAt!.Value.AddHours(3); }
                if (status >= ReportStatus.Collected)  { collectedAt = startAt!.Value.AddHours(2); }
                if (status == ReportStatus.Completed)  { completeAt = collectedAt!.Value.AddHours(1); updatedAt = completeAt; }
                if (status == ReportStatus.Rejected)   { updatedAt = reportAt.AddHours(4); }
                if (status == ReportStatus.Failed)     { updatedAt = startAt?.AddHours(3) ?? reportAt.AddHours(5); }

                return new CitizenReport
                {
                    Id              = Guid.NewGuid(),
                    UserId          = citizenId,
                    EnterpriseId    = org.EnterpriseId,
                    Status          = status,
                    Types           = types ?? [WasteType.Organic],
                    Capacity        = capacity ?? 5m,
                    ActualCapacityKg = status >= ReportStatus.Collected ? (capacity ?? 5m) * 0.9m : null,
                    Description     = $"Báo cáo rác tại địa chỉ - ngày -{daysAgo}",
                    Address         = "Đường test, Quận 3",
                    TeamId          = teamId,
                    AssignBy        = assignBy,
                    AssignAt        = assignAt,
                    Deadline        = deadline,
                    StartCollectingAt = startAt,
                    CollectedAt     = collectedAt,
                    CompleteAt      = completeAt,
                    Point           = status == ReportStatus.Completed ? pointVal ?? 10 : null,
                    ReportNote      = note,
                    ReportAt        = reportAt,
                    CreatedAt       = reportAt,
                    UpdatedAt       = updatedAt ?? reportAt,
                    CitizenImageUrls = [$"https://storage.example.com/reports/{Guid.NewGuid()}.jpg"],
                    CollectorImageUrls = status >= ReportStatus.Collected
                        ? [$"https://storage.example.com/collected/{Guid.NewGuid()}.jpg"] : []
                };
            }

            var cits = usr.Cit;
            var (ta, tb, tg) = (col.TeamAId, col.TeamBId, col.TeamGId);
            var entId = usr.Ent1Id;

            // ── Pending (6 báo cáo mới nhất) ─────────────────────────────────
            for (int i = 0; i < 6; i++)
                reports.Add(R(cits[i % 10], ReportStatus.Pending, i,
                    types: i % 2 == 0 ? [WasteType.Organic] : [WasteType.Recyclable, WasteType.Organic],
                    capacity: 3m + i));

            // ── Queue (5 báo cáo đã vào hàng đợi) ───────────────────────────
            for (int i = 0; i < 5; i++)
                reports.Add(R(cits[i % 10], ReportStatus.Queue, 3 + i,
                    assignBy: entId, capacity: 4m + i));

            // ── Assigned (5 đã giao team) ─────────────────────────────────────
            reports.Add(R(cits[0], ReportStatus.Assigned, 5, teamId: ta, assignBy: entId, capacity: 6m));
            reports.Add(R(cits[1], ReportStatus.Assigned, 6, teamId: ta, assignBy: entId, capacity: 8m));
            reports.Add(R(cits[2], ReportStatus.Assigned, 7, teamId: tb, assignBy: entId, capacity: 5m));
            reports.Add(R(cits[3], ReportStatus.Assigned, 8, teamId: tb, assignBy: entId, capacity: 7m));
            reports.Add(R(cits[4], ReportStatus.Assigned, 9, teamId: tg, assignBy: entId, capacity: 4m));

            // ── Processing (4 đang thu gom) ───────────────────────────────────
            reports.Add(R(cits[5], ReportStatus.Processing, 10, teamId: ta, assignBy: entId, capacity: 9m));
            reports.Add(R(cits[6], ReportStatus.Processing, 11, teamId: tb, assignBy: entId, capacity: 6m));
            reports.Add(R(cits[7], ReportStatus.Processing, 12, teamId: tg, assignBy: entId, capacity: 7m));
            reports.Add(R(cits[8], ReportStatus.Processing, 13, teamId: ta, assignBy: entId, capacity: 5m));

            // ── Collected (4 đã thu gom, chờ duyệt hoàn thành) ───────────────
            reports.Add(R(cits[9], ReportStatus.Collected, 14, teamId: ta, assignBy: entId, capacity: 10m));
            reports.Add(R(cits[0], ReportStatus.Collected, 15, teamId: tb, assignBy: entId, capacity: 8m));
            reports.Add(R(cits[1], ReportStatus.Collected, 16, teamId: tg, assignBy: entId, capacity: 12m));
            reports.Add(R(cits[2], ReportStatus.Collected, 17, teamId: ta, assignBy: entId, capacity: 6m));

            // ── Completed (20 hoàn thành — trải đều 30 ngày) ─────────────────
            var completedDays = new[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
            var completedTypes = new[]
            {
                new[] { WasteType.Organic }, new[] { WasteType.Recyclable }, new[] { WasteType.Organic, WasteType.Recyclable },
                new[] { WasteType.NonRecyclable }, new[] { WasteType.Organic }, new[] { WasteType.Recyclable },
                new[] { WasteType.Organic, WasteType.NonRecyclable }, new[] { WasteType.Recyclable }, new[] { WasteType.Organic },
                new[] { WasteType.Recyclable, WasteType.NonRecyclable }, new[] { WasteType.Organic }, new[] { WasteType.Recyclable },
                new[] { WasteType.Organic }, new[] { WasteType.Recyclable }, new[] { WasteType.Organic },
                new[] { WasteType.NonRecyclable }, new[] { WasteType.Organic }, new[] { WasteType.Recyclable },
                new[] { WasteType.Organic }, new[] { WasteType.Recyclable }
            };
            int[] caps  = { 5, 8, 12, 6, 9, 7, 4, 11, 6, 8, 5, 7, 10, 6, 9, 8, 5, 7, 6, 10 };
            int[] pts   = { 10, 15, 25, 5, 10, 15, 15, 20, 10, 20, 10, 15, 10, 15, 10, 5, 10, 15, 10, 15 };
            var teams   = new[] { ta, ta, tb, tb, tg, tg, ta, tb, tg, ta, tb, tg, ta, ta, tb, tg, ta, tb, tg, ta };

            for (int i = 0; i < completedDays.Length; i++)
                reports.Add(R(cits[i % 10], ReportStatus.Completed, completedDays[i],
                    teamId: teams[i], assignBy: entId,
                    pointVal: pts[i], capacity: caps[i],
                    types: completedTypes[i].ToList()));

            // ── Rejected (4 bị từ chối) ───────────────────────────────────────
            reports.Add(R(cits[3], ReportStatus.Rejected, 5,  note: "Hình ảnh không rõ, không xác định được địa chỉ"));
            reports.Add(R(cits[4], ReportStatus.Rejected, 9,  note: "Địa điểm ngoài khu vực phụ trách"));
            reports.Add(R(cits[5], ReportStatus.Rejected, 15, note: "Báo cáo trùng lặp với đơn đã xử lý"));
            reports.Add(R(cits[6], ReportStatus.Rejected, 21, note: "Loại rác không thuộc danh mục xử lý"));

            // ── Failed (4 thất bại) ───────────────────────────────────────────
            reports.Add(R(cits[7], ReportStatus.Failed, 7,  teamId: ta, assignBy: entId, note: "Không tiếp cận được địa điểm"));
            reports.Add(R(cits[8], ReportStatus.Failed, 13, teamId: tb, assignBy: entId, note: "Xe hỏng giữa đường"));
            reports.Add(R(cits[9], ReportStatus.Failed, 19, teamId: tg, assignBy: entId, note: "Nhà dân chặn lối vào"));
            reports.Add(R(cits[0], ReportStatus.Failed, 27, teamId: ta, assignBy: entId, note: "Không đủ nhân lực trong ca"));

            db.CitizenReports.AddRange(reports);
            await db.SaveChangesAsync();

            // ── Complaints (10 khiếu nại trên các báo cáo Completed/Failed) ──
            var completedReports = reports.Where(r => r.Status == ReportStatus.Completed).ToList();
            var failedReports    = reports.Where(r => r.Status == ReportStatus.Failed).ToList();

            var complaints = new List<Complaint>();

            // 6 khiếu nại Pending
            for (int i = 0; i < 6 && i < completedReports.Count; i++)
            {
                complaints.Add(new Complaint
                {
                    Id = Guid.NewGuid(), CitizenId = completedReports[i].UserId,
                    ReportId = completedReports[i].Id,
                    Reason = $"Nhân viên thu gom không đúng quy trình, bỏ sót rác không thu gom hết (lần {i + 1})",
                    ImageUrls = [$"https://storage.example.com/complaints/{Guid.NewGuid()}.jpg"],
                    Status = ComplaintStatus.Pending,
                    RequestAt = completedReports[i].CompleteAt!.Value.AddDays(1)
                });
            }
            // 2 khiếu nại đã phản hồi
            for (int i = 6; i < 8 && i < completedReports.Count; i++)
            {
                var req = completedReports[i].CompleteAt!.Value.AddDays(2);
                complaints.Add(new Complaint
                {
                    Id = Guid.NewGuid(), CitizenId = completedReports[i].UserId,
                    ReportId = completedReports[i].Id,
                    Reason = "Thời gian xử lý quá lâu, gây ảnh hưởng vệ sinh khu vực",
                    ImageUrls = [],
                    Status = ComplaintStatus.Approved,
                    AdminResponse = "Chúng tôi đã ghi nhận và sẽ cải thiện thời gian phản hồi trong tương lai.",
                    RequestAt = req, ResponseAt = req.AddDays(2)
                });
            }
            // 2 khiếu nại trên báo cáo Failed
            for (int i = 0; i < 2 && i < failedReports.Count; i++)
            {
                complaints.Add(new Complaint
                {
                    Id = Guid.NewGuid(), CitizenId = failedReports[i].UserId,
                    ReportId = failedReports[i].Id,
                    Reason = "Đơn bị đánh dấu thất bại nhưng thực tế chưa có ai đến thu gom",
                    ImageUrls = [$"https://storage.example.com/complaints/{Guid.NewGuid()}.jpg"],
                    Status = ComplaintStatus.Pending,
                    RequestAt = failedReports[i].UpdatedAt!.Value.AddDays(1)
                });
            }

            db.Complaints.AddRange(complaints);
            await db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper record types
        // ─────────────────────────────────────────────────────────────────────
        private record WorkAreaIds
        {
            public Guid DistrictQ3Id { get; init; }
            public Guid DistrictQ1Id { get; init; }
            public Guid WardP1Id     { get; init; }
            public Guid WardP2Id     { get; init; }
            public Guid WardP3Id     { get; init; }
            public Guid WardP4Id     { get; init; }
            public Guid WardQ1P1Id   { get; init; }
        }

        private record UserIds
        {
            public Guid   AdminId { get; init; }
            public Guid   Ent1Id  { get; init; }
            public Guid   Ent2Id  { get; init; }
            public Guid   Ent3Id  { get; init; }
            public Guid   Col1Id  { get; init; }
            public Guid   Col2Id  { get; init; }
            public Guid   Col3Id  { get; init; }
            public Guid[] Cit     { get; init; } = [];
        }

        private record OrgIds
        {
            public Guid EnterpriseId { get; init; }
        }

        private record ColIds
        {
            public Guid Col1Id { get; init; }
            public Guid Col2Id { get; init; }
            public Guid Hub1Id { get; init; }
            public Guid Hub2Id { get; init; }
            public Guid TeamAId { get; init; }
            public Guid TeamBId { get; init; }
            public Guid TeamGId { get; init; }
        }
    }
}
