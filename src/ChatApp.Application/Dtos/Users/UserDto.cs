namespace ChatApp.Application.Dtos.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public DateTime? LastSeenUtc { get; set; }
        public bool IsOnline { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}