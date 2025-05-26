using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string CreateToken(User user, IList<string>? roles = null);
    }
}