namespace ChatApp.Domain.Entities
{
    public class PrivateMessage : BaseEntity
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime TimestampUtc { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsFromAI { get; set; } = false;

        // Navigation Properties
        public virtual User Sender { get; set; } = null!;
        public virtual User Receiver { get; set; } = null!;
    }
}