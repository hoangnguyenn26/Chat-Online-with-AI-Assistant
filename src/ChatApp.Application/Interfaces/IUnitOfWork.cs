using ChatApp.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChatApp.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        IPrivateMessageRepository PrivateMessageRepository { get; }
        IUserConnectionRepository UserConnectionRepository { get; }
        // Thêm các Repository khác ở đây

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Transaction methods 
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
    }
}