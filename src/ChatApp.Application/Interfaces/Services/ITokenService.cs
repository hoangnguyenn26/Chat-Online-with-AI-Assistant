using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user, IEnumerable<string>? roles = null);
    }
}