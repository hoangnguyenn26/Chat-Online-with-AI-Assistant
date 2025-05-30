using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Interfaces.Repositories
{
    public interface IUserConnectionRepository
    {
        Task<UserConnection?> GetByIdAsync(string connectionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserConnection>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(UserConnection connection, CancellationToken cancellationToken = default);
        Task RemoveAsync(UserConnection connection, CancellationToken cancellationToken = default);
        Task RemoveRangeAsync(IEnumerable<UserConnection> connections, CancellationToken cancellationToken = default);
        Task<bool> UserHasConnectionsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}