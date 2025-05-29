using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Api.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ApplicationDbContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("SignalR: User connected without valid UserId in claims. ConnectionId: {ConnectionId}", Context.ConnectionId);
                await base.OnConnectedAsync();
                return;
            }

            _logger.LogInformation("SignalR: User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);

            // 1. Lưu ConnectionId và UserId vào UserConnections
            var existingConnection = await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.ConnectionId == Context.ConnectionId);

            if (existingConnection == null)
            {
                var userConnection = new UserConnection
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userId,
                    ConnectedAtUtc = DateTime.UtcNow
                };
                _context.UserConnections.Add(userConnection);
            }
            else
            {
                existingConnection.UserId = userId;
                existingConnection.ConnectedAtUtc = DateTime.UtcNow;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastSeenUtc = DateTime.UtcNow;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            await Clients.Others.SendAsync("UserOnline", userId.ToString());
            _logger.LogInformation("SignalR: Notified others that User {UserId} is online", userId);


            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = null;
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid parsedUserId))
            {
                userId = parsedUserId;
            }

            _logger.LogInformation("SignalR: User {UserId} (ConnectionId: {ConnectionId}) disconnected. Exception: {ExceptionMessage}",
                userId?.ToString() ?? "Unknown", Context.ConnectionId, exception?.Message ?? "N/A");

            // 1. Xóa ConnectionId khỏi UserConnections
            var userConnection = await _context.UserConnections
                .FirstOrDefaultAsync(uc => uc.ConnectionId == Context.ConnectionId);

            if (userConnection != null)
            {
                _context.UserConnections.Remove(userConnection);

                // 2. Cập nhật Users.LastSeenUtc
                bool stillHasOtherConnections = await _context.UserConnections
                    .AnyAsync(uc => uc.UserId == userConnection.UserId && uc.ConnectionId != Context.ConnectionId);

                if (!stillHasOtherConnections && userConnection.UserId != Guid.Empty)
                {
                    var user = await _context.Users.FindAsync(userConnection.UserId);
                    if (user != null)
                    {
                        user.LastSeenUtc = DateTime.UtcNow;
                        _context.Users.Update(user);
                    }
                }
                await _context.SaveChangesAsync();

                // 3. Thông báo cho các client khác user này đã offline
                if (!stillHasOtherConnections && userConnection.UserId != Guid.Empty)
                {
                    await Clients.Others.SendAsync("UserOffline", userConnection.UserId.ToString());
                    _logger.LogInformation("SignalR: Notified others that User {UserId} is offline (last connection).", userConnection.UserId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}