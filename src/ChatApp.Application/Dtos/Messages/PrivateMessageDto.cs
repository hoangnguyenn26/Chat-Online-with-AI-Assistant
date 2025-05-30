namespace ChatApp.Application.Dtos.Messages
{
    public class PrivateMessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderDisplayName { get; set; } = null!;
        public string? SenderAvatarUrl { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime TimestampUtc { get; set; }
        public bool IsFromAI { get; set; } = false;
        public bool IsRead { get; set; }
    }
}