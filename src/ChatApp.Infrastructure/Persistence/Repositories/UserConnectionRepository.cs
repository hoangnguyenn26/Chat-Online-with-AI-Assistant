using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces.Repositories;
using ChatApp.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories
{
    public class UserConnectionRepository : IUserConnectionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<UserConnection> _dbSet;

        public UserConnectionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<UserConnection>();
        }

        public async Task<UserConnection?> GetByIdAsync(string connectionId, CancellationToken cancellationToken = default)
        {
            // FindAsync dùng được cho PK kiểu string
            return await _dbSet.FindAsync(new object[] { connectionId }, cancellationToken);
        }

        public async Task<IEnumerable<UserConnection>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(uc => uc.UserId == userId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserConnection connection, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(connection, cancellationToken);
        }

        public Task RemoveAsync(UserConnection connection, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(connection);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync(IEnumerable<UserConnection> connections, CancellationToken cancellationToken = default)
        {
            _dbSet.RemoveRange(connections);
            return Task.CompletedTask;
        }

        public async Task<bool> UserHasConnectionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(uc => uc.UserId == userId, cancellationToken);
        }
    }
}