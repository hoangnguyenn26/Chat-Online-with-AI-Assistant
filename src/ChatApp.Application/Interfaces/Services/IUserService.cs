using ChatApp.Application.Dtos.Users;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<UserDto?> GetUserProfileByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IEnumerable<UserDto>> GetAllUsersAsync(
            Guid currentUserId,
            string? searchQuery = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<User> FindOrCreateUserFromOAuthAsync(
            string providerName,
            string externalId,
            string email,
            string displayName,
            string? avatarUrl,
            CancellationToken cancellationToken = default);

        Task UpdateUserPresenceAsync(Guid userId, bool isOnline, CancellationToken cancellationToken = default);
    }
}