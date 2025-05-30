using ChatApp.Application.Interfaces;
using ChatApp.Domain.Interfaces.Repositories;
using ChatApp.Infrastructure.Persistence.DbContext;
using ChatApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChatApp.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IUserRepository? _userRepository;
        private IPrivateMessageRepository? _privateMessageRepository;
        private IUserConnectionRepository? _userConnectionRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);
        public IPrivateMessageRepository PrivateMessageRepository => _privateMessageRepository ??= new PrivateMessageRepository(_context);
        public IUserConnectionRepository UserConnectionRepository => _userConnectionRepository ??= new UserConnectionRepository(_context);
        // Khởi tạo các Repository khác tương tự

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            try
            {
                await SaveChangesAsync(cancellationToken); // Đảm bảo SaveChanges được gọi trước khi commit
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(transaction, cancellationToken);
                throw;
            }
        }

        public async Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            await transaction.RollbackAsync(cancellationToken);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}