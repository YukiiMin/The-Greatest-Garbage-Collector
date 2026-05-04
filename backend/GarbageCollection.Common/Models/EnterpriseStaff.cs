namespace GarbageCollection.Common.Models
{
    public class EnterpriseStaff
    {
        public Guid      UserId       { get; set; }
        public Guid      EnterpriseId { get; set; }
        public DateTime? JoinHubAt    { get; set; }

        // Navigation
        public User       User       { get; set; } = null!;
        public Enterprise Enterprise { get; set; } = null!;
    }
}
