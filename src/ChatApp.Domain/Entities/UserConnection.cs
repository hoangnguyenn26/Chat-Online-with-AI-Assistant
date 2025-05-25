namespace ChatApp.Domain.Entities
{
    public class UserConnection
    {
        public string ConnectionId { get; set; } = null!; // PK - SignalR Connection ID
        public Guid UserId { get; set; }
        public DateTime ConnectedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual User User { get; set; } = null!;
    }
}