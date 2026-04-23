namespace GarbageCollection.Common.Models
{
    public class Staff
    {
        public Guid UserId { get; set; }
        public Guid EnterpriseId { get; set; }
        public Guid TeamId { get; set; }
        public DateTime? JoinTeamAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Enterprise Enterprise { get; set; } = null!;
        public Team Team { get; set; } = null!;
    }
}
