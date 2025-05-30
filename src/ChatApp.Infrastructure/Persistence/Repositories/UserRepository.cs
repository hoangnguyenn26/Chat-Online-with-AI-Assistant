using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces.Repositories;
using ChatApp.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ChatApp.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default, bool tracking = false)
        {
            IQueryable<User> query = _dbSet;
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByExternalIdAsync(string providerName, string externalId, CancellationToken cancellationToken = default, bool tracking = false)
        {
            IQueryable<User> query = _dbSet;
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync(u => u.ProviderName == providerName && u.ExternalId == externalId, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersAsync(
            Expression<Func<User, bool>>? filter = null,
            Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null,
            string? includeProperties = null, // Ví dụ: "UserRoles.Role"
            int page = 1,
            int pageSize = 20,
            bool tracking = false,
            CancellationToken cancellationToken = default)
        {
            return await base.ListAsync(filter, orderBy ?? (q => q.OrderBy(u => u.DisplayName)), includeProperties, tracking, page, pageSize, cancellationToken);
        }
    }
}