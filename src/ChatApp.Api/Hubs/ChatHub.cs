using ChatApp.Application.Dtos.Messages;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Api.Hubs
{
    [Authorize]
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

        public async Task SendPrivateMessage(string receiverUserIdString, string messageContent)
        {
            var senderIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderIdString) || !Guid.TryParse(senderIdString, out Guid senderId))
            {
                _logger.LogWarning("SendPrivateMessage: SenderId not found or invalid in claims for ConnectionId {ConnectionId}.", Context.ConnectionId);
                // await Clients.Caller.SendAsync("ReceiveMessageError", "Authentication required to send messages.");
                return;
            }

            if (string.IsNullOrEmpty(receiverUserIdString) || !Guid.TryParse(receiverUserIdString, out Guid receiverId))
            {
                _logger.LogWarning("SendPrivateMessage: Invalid ReceiverUserId format '{ReceiverUserIdString}' from Sender {SenderId}.", receiverUserIdString, senderId);
                // await Clients.Caller.SendAsync("ReceiveMessageError", "Invalid recipient ID.");
                return;
            }

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                _logger.LogWarning("SendPrivateMessage: Empty message content from Sender {SenderId} to Receiver {ReceiverId}.", senderId, receiverId);
                // await Clients.Caller.SendAsync("ReceiveMessageError", "Message content cannot be empty.");
                return;
            }

            if (senderId == receiverId)
            {
                _logger.LogWarning("SendPrivateMessage: Sender {SenderId} attempting to send message to themselves.", senderId);
                // await Clients.Caller.SendAsync("ReceiveMessageError", "Cannot send message to yourself in this context.");
                return;
            }

            _logger.LogInformation("User {SenderId} sending private message to {ReceiverId}: '{Content}'", senderId, receiverId, messageContent.Length > 20 ? messageContent.Substring(0, 20) + "..." : messageContent);

            var sender = await _context.Users.FindAsync(senderId);
            if (sender == null)
            {
                _logger.LogError("SendPrivateMessage: Sender user with Id {SenderId} not found in database.", senderId);
                return;
            }

            var privateMessage = new PrivateMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = messageContent,
                TimestampUtc = DateTime.UtcNow,
                IsFromAI = false,
                IsRead = false
            };

            _context.PrivateMessages.Add(privateMessage);
            await _context.SaveChangesAsync();

            var messageDto = new PrivateMessageDto
            {
                Id = privateMessage.Id,
                SenderId = sender.Id,
                SenderDisplayName = sender.DisplayName,
                SenderAvatarUrl = sender.AvatarUrl,
                ReceiverId = receiverId,
                Content = privateMessage.Content,
                TimestampUtc = privateMessage.TimestampUtc,
                IsFromAI = privateMessage.IsFromAI,
                IsRead = privateMessage.IsRead
            };

            var receiverConnections = await _context.UserConnections
                .Where(uc => uc.UserId == receiverId)
                .Select(uc => uc.ConnectionId)
                .ToListAsync();

            if (receiverConnections.Any())
            {
                _logger.LogInformation("Sending message {MessageId} to Receiver {ReceiverId} connections: {ConnectionIds}", messageDto.Id, receiverId, string.Join(", ", receiverConnections));
                await Clients.Clients(receiverConnections).SendAsync("ReceivePrivateMessage", messageDto);
            }
            else
            {
                _logger.LogInformation("Receiver {ReceiverId} is not currently connected. Message {MessageId} stored.", receiverId, messageDto.Id);
            }
            _logger.LogInformation("Sending message {MessageId} back to Sender {SenderId} (Connection: {ConnectionId})", messageDto.Id, senderId, Context.ConnectionId);
            await Clients.Caller.SendAsync("ReceivePrivateMessage", messageDto);
        }
    }
}