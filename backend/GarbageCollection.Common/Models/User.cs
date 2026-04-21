using GarbageCollection.Common.Enums;

namespace GarbageCollection.Common.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public bool EmailVerified { get; set; }
        public string? GoogleId { get; set; }
        public string Provider { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsBanned { get; set; }
        public bool IsLogin { get; set; }
        public int LoginTerm { get; set; }
        public UserRole Role { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
