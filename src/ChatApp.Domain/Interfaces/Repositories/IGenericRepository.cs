// src/ChatApp.Domain/Interfaces/Repositories/IGenericRepository.cs
using ChatApp.Domain.Entities; // Cần BaseEntity
using System.Linq.Expressions;

namespace ChatApp.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, bool tracking = false);
        Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default, bool tracking = false);
        Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null,
            bool isTracking = false,
            int? page = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default);

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);
    }
}