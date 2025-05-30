using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces.Repositories;
using ChatApp.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ChatApp.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, bool tracking = false)
        {
            IQueryable<T> query = _dbSet;
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            // FirstOrDefaultAsync an toàn hơn FindAsync khi dùng với AsNoTracking và điều kiện phức tạp hơn
            return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default, bool tracking = false)
        {
            IQueryable<T> query = _dbSet;
            if (!tracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null,
            bool isTracking = false,
            int? page = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (!isTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken, tracking: true);
            if (entity != null)
            {
                await DeleteAsync(entity, cancellationToken);
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.CountAsync(cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                return await query.AnyAsync(filter, cancellationToken);
            }
            return await query.AnyAsync(cancellationToken);
        }
    }
}