namespace GarbageCollection.Common.Models
{
    public class Staff
    {
        public Guid UserId { get; set; }
        public int EnterpriseId { get; set; }
        public int TeamId { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Enterprise Enterprise { get; set; } = null!;
        public Team Team { get; set; } = null!;
    }
}
