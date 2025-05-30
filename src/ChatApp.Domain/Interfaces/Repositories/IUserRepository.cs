using ChatApp.Domain.Entities;
using System.Linq.Expressions;

namespace ChatApp.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default, bool tracking = false);
        Task<User?> GetByExternalIdAsync(string providerName, string externalId, CancellationToken cancellationToken = default, bool tracking = false);
        Task<IEnumerable<User>> GetUsersAsync(
            Expression<Func<User, bool>>? filter = null,
            Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null,
            string? includeProperties = null,
            int page = 1,
            int pageSize = 20,
            bool tracking = false,
            CancellationToken cancellationToken = default);
    }
}