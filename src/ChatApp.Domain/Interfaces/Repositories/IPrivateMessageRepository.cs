using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Interfaces.Repositories
{
    public interface IPrivateMessageRepository : IGenericRepository<PrivateMessage>
    {
        Task<IEnumerable<PrivateMessage>> GetMessageHistoryAsync(
            Guid userId1,
            Guid userId2,
            DateTime? beforeTimestampUtc = null,
            int limit = 20,
            CancellationToken cancellationToken = default);
    }
}