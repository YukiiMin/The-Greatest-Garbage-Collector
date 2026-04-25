using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GarbageCollection.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CitizenReport> CitizenReports { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Enterprise> Enterprises { get; set; }
        public DbSet<Collector> Collectors { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<PointCategory> PointCategories { get; set; }


        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();

        public DbSet<PasswordOtp> PasswordOtps => Set<PasswordOtp>();
        public DbSet<UserPoints> UserPoints => Set<UserPoints>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CitizenReport>(entity =>
            {
                entity.ToTable("citizen_reports");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CitizenImageUrls)
                      .IsRequired()
                      .HasColumnType("text")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      )
                      .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                          (a, b) => a != null && b != null && a.SequenceEqual(b),
                          v => v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
                          v => v.ToList()
                      ));

                entity.Property(e => e.CollectorImageUrls)
                      .IsRequired()
                      .HasColumnType("text")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      )
                      .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                          (a, b) => a != null && b != null && a.SequenceEqual(b),
                          v => v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
                          v => v.ToList()
                      ));

                entity.Property(e => e.Types)
                      .IsRequired()
                      .HasColumnType("text")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<WasteType>>(v, (JsonSerializerOptions?)null) ?? new List<WasteType>()
                      )
                      .Metadata.SetValueComparer(new ValueComparer<List<WasteType>>(
                          (a, b) => a != null && b != null && a.SequenceEqual(b),
                          v => v.Aggregate(0, (acc, w) => HashCode.Combine(acc, w.GetHashCode())),
                          v => v.ToList()
                      ));

                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ReportNote).HasMaxLength(500);
                entity.Property(e => e.Capacity).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ActualCapacityKg).HasColumnName("actual_capacity_kg").HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.UserId).HasColumnName("citizen_id");
                entity.Property(e => e.AssignBy).HasColumnName("assign_by");
                entity.Property(e => e.AssignAt).HasColumnName("assign_at");
                entity.Property(e => e.Deadline).HasColumnName("deadline");
                entity.Property(e => e.StartCollectingAt).HasColumnName("start_collecting_at");
                entity.Property(e => e.CollectedAt).HasColumnName("collected_at");
                entity.Property(e => e.ReportAt).HasColumnName("report_at");
                entity.Property(e => e.CompleteAt).HasColumnName("complete_at");
                entity.Property(e => e.TeamId).HasColumnName("team_id");

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Complaint>(entity =>
            {
                entity.ToTable("complaints");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CitizenId).HasColumnName("citizen_id");
                entity.Property(e => e.ReportId).HasColumnName("report_id");
                entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(1000);
                entity.Property(e => e.AdminResponse).HasColumnName("admin_response");
                entity.Property(e => e.RequestAt).HasColumnName("request_at");
                entity.Property(e => e.ResponseAt).HasColumnName("response_at");

                entity.Property(e => e.ImageUrls)
                      .HasColumnName("image_urls")
                      .HasColumnType("text")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      )
                      .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                          (a, b) => a != null && b != null && a.SequenceEqual(b),
                          v => v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
                          v => v.ToList()
                      ));

                entity.Property(e => e.Status)
                      .HasColumnName("status")
                      .HasConversion<string>();

                entity.Property(e => e.Messages)
                      .HasColumnName("messages")
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<ComplaintMessage>>(v, (JsonSerializerOptions?)null) ?? new List<ComplaintMessage>()
                      )
                      .Metadata.SetValueComparer(new ValueComparer<List<ComplaintMessage>>(
                          (a, b) => a != null && b != null && a.Count == b.Count,
                          v => v.Count,
                          v => v.ToList()
                      ));

                entity.HasOne(e => e.Report)
                      .WithMany()
                      .HasForeignKey(e => e.ReportId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Citizen)
                      .WithMany()
                      .HasForeignKey(e => e.CitizenId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // ── PointCategory ─────────────────────────────────────────────────
            modelBuilder.Entity<PointCategory>(e =>
            {
                e.ToTable("point_categories");
                e.HasKey(p => p.Id);

                e.Property(p => p.Id).HasColumnName("id");
                e.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(256);
                e.Property(p => p.EnterpriseId).HasColumnName("enterprise_id");
                e.Property(p => p.IsActive).HasColumnName("is_active");
                e.Property(p => p.CreatedAt).HasColumnName("created_at");
                e.Property(p => p.UpdatedAt).HasColumnName("updated_at");

                e.Property(p => p.Mechanic)
                 .HasColumnName("mechanic")
                 .HasColumnType("jsonb")
                 .HasConversion(
                     v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                     v => System.Text.Json.JsonSerializer.Deserialize<PointMechanic>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new PointMechanic()
                 );

                e.HasOne(p => p.Enterprise)
                 .WithMany()
                 .HasForeignKey(p => p.EnterpriseId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Staff ─────────────────────────────────────────────────────────
            modelBuilder.Entity<Staff>(e =>
            {
                e.ToTable("staffs");
                e.HasKey(s => s.UserId);

                e.Property(s => s.UserId).HasColumnName("user_id");
                e.Property(s => s.EnterpriseId).HasColumnName("enterprise_id");
                e.Property(s => s.TeamId).HasColumnName("team_id");

                e.HasOne(s => s.User)
                 .WithMany()
                 .HasForeignKey(s => s.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(s => s.Enterprise)
                 .WithMany()
                 .HasForeignKey(s => s.EnterpriseId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(s => s.Team)
                 .WithMany()
                 .HasForeignKey(s => s.TeamId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Enterprise ────────────────────────────────────────────────────
            modelBuilder.Entity<Enterprise>(e =>
            {
                e.ToTable("enterprises");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(256);
                e.Property(x => x.PhoneNumber).HasColumnName("phone_number").IsRequired().HasMaxLength(20);
                e.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                e.Property(x => x.Address).HasColumnName("address").IsRequired().HasMaxLength(512);
                e.Property(x => x.Latitude).HasColumnName("latitude").HasColumnType("decimal(9,6)");
                e.Property(x => x.Longitude).HasColumnName("longitude").HasColumnType("decimal(9,6)");
                e.Property(x => x.WorkArea).HasColumnName("work_area").IsRequired();
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

                e.HasIndex(x => x.Email).IsUnique();
            });

            // ── Collector ─────────────────────────────────────────────────────
            modelBuilder.Entity<Collector>(e =>
            {
                e.ToTable("collectors");
                e.HasKey(c => c.Id);

                e.Property(c => c.Id).HasColumnName("id");
                e.Property(c => c.Name).HasColumnName("name").IsRequired().HasMaxLength(256);
                e.Property(c => c.PhoneNumber).HasColumnName("phone_number").IsRequired().HasMaxLength(20);
                e.Property(c => c.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                e.Property(c => c.Address).HasColumnName("address").IsRequired().HasMaxLength(512);
                e.Property(c => c.Latitude).HasColumnName("latitude").HasColumnType("decimal(9,6)");
                e.Property(c => c.Longitude).HasColumnName("longitude").HasColumnType("decimal(9,6)");
                e.Property(c => c.WorkArea).HasColumnName("work_area").IsRequired();
                e.Property(c => c.AssignedCapacity).HasColumnName("assigned_capacity");
                e.Property(c => c.EnterpriseId).HasColumnName("enterprise_id");
                e.Property(c => c.CreatedAt).HasColumnName("created_at");
                e.Property(c => c.UpdatedAt).HasColumnName("updated_at");

                e.HasIndex(c => c.Email).IsUnique();
                e.HasIndex(c => c.EnterpriseId);

                e.HasOne(c => c.Enterprise)
                 .WithMany()
                 .HasForeignKey(c => c.EnterpriseId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("fk_collectors_enterprise_id");
            });

            // ── Team ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Team>(e =>
            {
                e.ToTable("teams");
                e.HasKey(t => t.Id);

                e.Property(t => t.Id).HasColumnName("id");
                e.Property(t => t.Name).HasColumnName("name").IsRequired().HasMaxLength(256);
                e.Property(t => t.TotalCapacity).HasColumnName("total_capacity").HasColumnType("decimal(10,2)");
                e.Property(t => t.IsActive).HasColumnName("is_active");
                e.Property(t => t.CollectorId).HasColumnName("collector_id");
                e.Property(t => t.WorkAreaId).HasColumnName("work_area_id");
                e.Property(t => t.DispatchTime).HasColumnName("dispatch_time").HasMaxLength(10);
                e.Property(t => t.RouteOptimized).HasColumnName("route_optimized");
                e.Property(t => t.InWork).HasColumnName("in_work");
                e.Property(t => t.StartWorkingTime).HasColumnName("start_working_time");
                e.Property(t => t.LastFinishTime).HasColumnName("last_finish_time");
                e.Property(t => t.CreatedAt).HasColumnName("created_at");
                e.Property(t => t.UpdatedAt).HasColumnName("updated_at");

                e.HasOne(t => t.Collector)
                 .WithMany()
                 .HasForeignKey(t => t.CollectorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(u => u.Id);

                e.Property(u => u.Id).HasColumnName("id");
                e.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                e.Property(u => u.EmailVerified).HasColumnName("email_verified");
                e.Property(u => u.GoogleId).HasColumnName("google_id").HasMaxLength(128);
                e.Property(u => u.Provider).HasColumnName("provider").IsRequired().HasMaxLength(64);
                e.Property(u => u.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(256);
                e.Property(u => u.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(1024);
                e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(512);
                e.Property(u => u.IsBanned).HasColumnName("is_banned");
                e.Property(u => u.IsLogin).HasColumnName("is_login");
                e.Property(u => u.LoginTerm).HasColumnName("login_term");
                e.Property(u => u.Role).HasColumnName("role").IsRequired().HasMaxLength(64);
                e.Property(u => u.Address).HasColumnName("address").HasMaxLength(512);
                e.Property(u => u.WorkArea).HasColumnName("work_area");
                e.Property(u => u.Area).HasColumnName("area");
                e.Property(u => u.CreatedAt).HasColumnName("created_at");
                e.Property(u => u.UpdatedAt).HasColumnName("updated_at");

                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.GoogleId);
            });

            // ── RefreshToken ─────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.ToTable("refresh_tokens");
                e.HasKey(rt => rt.Id);

                e.Property(rt => rt.Id).HasColumnName("id");
                e.Property(rt => rt.UserId).HasColumnName("user_id");
                e.Property(rt => rt.TokenHash).HasColumnName("token_hash").IsRequired().HasMaxLength(128);
                e.Property(rt => rt.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                e.Property(rt => rt.ExpiresAt).HasColumnName("expires_at");
                e.Property(rt => rt.IsRevoked).HasColumnName("is_revoked");
                e.Property(rt => rt.CreatedAt).HasColumnName("created_at");

                e.HasOne(rt => rt.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(rt => rt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(rt => rt.TokenHash).IsUnique();
            });
            // ── EmailOtp ──────────────────────────────────────────────────────
            modelBuilder.Entity<EmailOtp>(e =>
            {
                e.ToTable("email_otps");
                e.HasKey(o => o.Id);

                e.Property(o => o.Id).HasColumnName("id");
                e.Property(o => o.Email).HasColumnName("email").IsRequired().HasMaxLength(320);
                e.Property(o => o.OtpCode).HasColumnName("otp_code").IsRequired().HasMaxLength(6);
                e.Property(o => o.ExpiresAt).HasColumnName("expires_at");
                e.Property(o => o.IsUsed).HasColumnName("is_used");
                e.Property(o => o.CreatedAt).HasColumnName("created_at");

                e.HasIndex(o => o.Email);
            });

            // ── UserPoints ────────────────────────────────────────────────────
            modelBuilder.Entity<UserPoints>(e =>
            {
                e.ToTable("user_points");
                e.HasKey(p => p.UserId);

                e.Property(p => p.UserId).HasColumnName("user_id");
                e.Property(p => p.WeekPoints).HasColumnName("week_points");
                e.Property(p => p.MonthPoints).HasColumnName("month_points");
                e.Property(p => p.YearPoints).HasColumnName("year_points");
                e.Property(p => p.TotalPoints).HasColumnName("total_points");
                e.Property(p => p.LeaderboardOptOut).HasColumnName("leaderboard_opt_out");
                e.Property(p => p.WorkAreaName).HasColumnName("work_area_name").HasMaxLength(256);
                e.Property(p => p.UpdatedAt).HasColumnName("updated_at");

                e.HasOne(p => p.User)
                 .WithOne()
                 .HasForeignKey<UserPoints>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<PasswordOtp>(e =>
            {
                e.ToTable("password_otp");

                // Email is the primary key per the DB schema.
                // One row per user — upserted on every password-reset request.
                e.HasKey(o => o.Email);

                e.Property(o => o.Email)
                 .HasColumnName("email")
                 .IsRequired()
                 .HasMaxLength(255);

                // OtpCode stores a BCrypt hash (≈60 chars) — never raw plain-text.
                e.Property(o => o.OtpCode)
                 .HasColumnName("otp_code")
                 .IsRequired()
                 .HasMaxLength(72);

                e.Property(o => o.ExpiresAt).HasColumnName("expires_at");
                e.Property(o => o.IsUsed).HasColumnName("is_used");
                e.Property(o => o.Count).HasColumnName("count");
                e.Property(o => o.CreatedAt).HasColumnName("created_at");
                e.Property(o => o.LastUpdatedAt).HasColumnName("last_updated_at");

                // FK: password_otp.email → users.email  ON DELETE CASCADE
                e.HasOne(o => o.User)
                 .WithOne(u => u.PasswordOtp)
                 .HasForeignKey<PasswordOtp>(o => o.Email)
                 .HasPrincipalKey<User>(u => u.Email)
                 .OnDelete(DeleteBehavior.Cascade);
            });

          
        }
    }
}
