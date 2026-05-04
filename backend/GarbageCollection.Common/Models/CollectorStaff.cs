namespace GarbageCollection.Common.Models
{
    /// <summary>Nhân viên thu gom (collector staff) — join table User → Collector org + CollectorHub + Team.</summary>
    public class CollectorStaff
    {
        public Guid  UserId         { get; set; }  // PK + FK → User
        public Guid  CollectorId    { get; set; }  // FK → Collector (org)
        public Guid? CollectorHubId { get; set; }  // FK → CollectorHub (optional)
        public Guid? TeamId         { get; set; }  // FK → Team (optional)
        public DateTime? JoinTeamAt { get; set; }

        // Navigation
        public User         User         { get; set; } = null!;
        public Collector    Collector    { get; set; } = null!;
        public CollectorHub? CollectorHub { get; set; }
        public Team?        Team         { get; set; }
    }
}
