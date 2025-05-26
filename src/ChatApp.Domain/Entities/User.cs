namespace ChatApp.Domain.Entities
{
    public class User : BaseEntity
    {
        public string? ExternalId { get; set; } // ID từ OAuth provider (Google User ID)
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? ProviderName { get; set; } // "Google", "Facebook"
        public DateTime? LastSeenUtc { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<UserConnection> Connections { get; set; } = new List<UserConnection>();
        public virtual ICollection<PrivateMessage> SentMessages { get; set; } = new List<PrivateMessage>();
        public virtual ICollection<PrivateMessage> ReceivedMessages { get; set; } = new List<PrivateMessage>();
    }
}