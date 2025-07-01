using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace ChatApp.Infrastructure.Persistence.DbContext
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<PrivateMessage> PrivateMessages { get; set; } = null!;
        public DbSet<UserConnection> UserConnections { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // User Configuration
            builder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ExternalId).HasMaxLength(256);
                entity.HasIndex(e => e.ExternalId); // Index cho ExternalId
                entity.Property(e => e.ProviderName).HasMaxLength(50);
                entity.Property(e => e.AvatarUrl).HasMaxLength(2048);
            });

            // PrivateMessage Configuration
            builder.Entity<PrivateMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.TimestampUtc).IsRequired();
                entity.HasIndex(e => new { e.SenderId, e.ReceiverId, e.TimestampUtc });

                // Mối quan hệ với Sender
                entity.HasOne(d => d.Sender)
                    .WithMany(p => p.SentMessages)
                    .HasForeignKey(d => d.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Mối quan hệ với Receiver
                entity.HasOne(d => d.Receiver)
                    .WithMany(p => p.ReceivedMessages)
                    .HasForeignKey(d => d.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // UserConnection Configuration
            builder.Entity<UserConnection>(entity =>
            {
                entity.HasKey(e => e.ConnectionId); // Đặt ConnectionId làm PK
                entity.HasIndex(e => e.UserId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Connections)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa connection nếu User bị xóa
            });

            // ----- SEED DATA CHO AI USER -----
            var aiUserId = new Guid("C82DCC58-9F57-4419-8439-94DFF46DBA5A"); // Guid cố định cho AI
            var now = DateTime.UtcNow;
            var seedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<User>().HasData(
                new User
                {
                    Id = aiUserId,
                    Email = "ai@chatapp.system", // Email duy nhất cho AI
                    DisplayName = "AI Assistant",
                    IsActive = true, // AI luôn active
                    ExternalId = null, // Không có ExternalId
                    ProviderName = "System", // Hoặc để null
                    AvatarUrl = null, // Có thể thêm avatar cho AI sau
                    LastSeenUtc = null, // AI không "seen"
                    CreatedAtUtc = seedTimestamp, // Thời điểm seed
                    UpdatedAtUtc = seedTimestamp
                }
            );
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAtUtc = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedAtUtc = DateTime.UtcNow;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}