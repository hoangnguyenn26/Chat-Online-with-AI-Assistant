using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces.Repositories;
using ChatApp.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories
{
    public class PrivateMessageRepository : GenericRepository<PrivateMessage>, IPrivateMessageRepository
    {
        public PrivateMessageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PrivateMessage>> GetMessageHistoryAsync(
            Guid userId1,
            Guid userId2,
            DateTime? beforeTimestampUtc = null,
            int limit = 20,
            CancellationToken cancellationToken = default)
        {
            IQueryable<PrivateMessage> query = _dbSet
                .Where(pm => (pm.SenderId == userId1 && pm.ReceiverId == userId2) ||
                             (pm.SenderId == userId2 && pm.ReceiverId == userId1))
                .Include(pm => pm.Sender);

            if (beforeTimestampUtc.HasValue)
            {
                query = query.Where(pm => pm.TimestampUtc < beforeTimestampUtc.Value);
            }
            return await query
                .OrderByDescending(pm => pm.TimestampUtc)
                .Take(limit)
                .AsNoTracking() // Read-only
                .ToListAsync(cancellationToken);
        }
    }
}