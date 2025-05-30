using ChatApp.Application.Dtos.Users;

namespace ChatApp.Application.Dtos.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public UserDto User { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}