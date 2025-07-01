using AutoMapper;
using ChatApp.Application.Dtos.Messages;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Interfaces.Services;
using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IAIService _aiService;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatHub> _logger;

        private static readonly Guid AI_USER_ID_PLACEHOLDER = new Guid("00000000-0000-0000-0000-0000000000AI");
        private const string AI_DISPLAY_NAME = "AI Assistant";


        public ChatHub(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IAIService aiService,
            IMapper mapper,
            ILogger<ChatHub> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task OnConnectedAsync()
        {
            var userIdString = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("SignalR OnConnectedAsync: UserId not found or invalid in claims. ConnectionId: {ConnectionId}", Context.ConnectionId);
                Context.Abort();
                return;
            }

            _logger.LogInformation("SignalR: User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);

            var userConnection = new UserConnection { ConnectionId = Context.ConnectionId, UserId = userId, ConnectedAtUtc = DateTime.UtcNow };
            await _unitOfWork.UserConnectionRepository.AddAsync(userConnection);
            await _userService.UpdateUserPresenceAsync(userId, isOnline: true);
            await _unitOfWork.SaveChangesAsync(); // Save UserConnection and User presence update

            await Clients.Others.SendAsync("UserOnline", userId.ToString());
            _logger.LogInformation("SignalR: Notified others that User {UserId} is online", userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var connection = await _unitOfWork.UserConnectionRepository.GetByIdAsync(connectionId);

            if (connection != null)
            {
                var userId = connection.UserId;
                _logger.LogInformation("SignalR: User {UserId} (ConnectionId: {ConnectionId}) disconnected. Exception: {ExceptionMessage}",
                    userId, connectionId, exception?.Message ?? "N/A");

                await _unitOfWork.UserConnectionRepository.RemoveAsync(connection);

                bool stillHasOtherConnections = await _unitOfWork.UserConnectionRepository.UserHasConnectionsAsync(userId);
                if (!stillHasOtherConnections)
                {
                    await _userService.UpdateUserPresenceAsync(userId, isOnline: false);
                    await Clients.Others.SendAsync("UserOffline", userId.ToString());
                    _logger.LogInformation("SignalR: Notified others that User {UserId} is offline (last connection).", userId);
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("SignalR OnDisconnectedAsync: ConnectionId {ConnectionId} not found in repository.", connectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrivateMessage(string receiverUserIdString, string messageContent)
        {
            var senderIdString = Context.UserIdentifier;
            if (!Guid.TryParse(senderIdString, out Guid senderId) || !Guid.TryParse(receiverUserIdString, out Guid receiverId))
            {
                _logger.LogWarning("SendPrivateMessage: Invalid SenderId or ReceiverUserId format.");
                return;
            }
            if (string.IsNullOrWhiteSpace(messageContent) || senderId == receiverId)
            {
                _logger.LogWarning("SendPrivateMessage: Empty message or sender is receiver. Sender: {SenderId}", senderId);
                return;
            }

            var sender = await _unitOfWork.UserRepository.GetByIdAsync(senderId, tracking: false);
            if (sender == null)
            {
                _logger.LogError("SendPrivateMessage: Sender user {SenderId} not found.", senderId);
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

            await _unitOfWork.PrivateMessageRepository.AddAsync(privateMessage);
            await _unitOfWork.SaveChangesAsync();

            var messageDto = _mapper.Map<PrivateMessageDto>(privateMessage);
            messageDto.SenderDisplayName = sender.DisplayName;
            messageDto.SenderAvatarUrl = sender.AvatarUrl;

            var receiverConnections = (await _unitOfWork.UserConnectionRepository.GetByUserIdAsync(receiverId))
                                       .Select(uc => uc.ConnectionId)
                                       .ToList();
            if (receiverConnections.Any())
            {
                await Clients.Clients(receiverConnections).SendAsync("ReceivePrivateMessage", messageDto);
            }
            await Clients.Caller.SendAsync("ReceivePrivateMessage", messageDto);
        }

        // src/ChatApp.Api/Hubs/ChatHub.cs

        public async Task AskAIInPrivateChat(string chatPartnerIdString, string userQuestion)
        {
            var currentUserIdString = Context.UserIdentifier;
            if (!Guid.TryParse(currentUserIdString, out Guid currentUserId) ||
                !Guid.TryParse(chatPartnerIdString, out Guid chatPartnerId) ||
                string.IsNullOrWhiteSpace(userQuestion))
            {
                _logger.LogWarning("AskAIInPrivateChat: Invalid parameters.");
                return;
            }

            _logger.LogInformation("User {UserId} asking AI in chat with {PartnerId}: '{QuestionStart}'",
                currentUserId, chatPartnerId, userQuestion.Substring(0, Math.Min(userQuestion.Length, 50)));

            // Lấy thông tin người dùng đang hỏi (cần cho việc tạo DTO)
            var userAsking = await _unitOfWork.UserRepository.GetByIdAsync(currentUserId, tracking: false);
            if (userAsking == null)
            {
                _logger.LogError("AskAI: User {UserId} not found for sending question message.", currentUserId);
                return;
            }

            // --- Bắt đầu Transaction (nếu cần, để đảm bảo tất cả tin nhắn được lưu cùng lúc) ---
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Lưu câu hỏi của User vào DB
                var userQuestionAsMessage = new PrivateMessage
                {
                    SenderId = currentUserId,
                    ReceiverId = chatPartnerId,
                    Content = $"(Question for AI): {userQuestion}",
                    TimestampUtc = DateTime.UtcNow,
                    IsFromAI = false,
                    IsRead = false
                };
                await _unitOfWork.PrivateMessageRepository.AddAsync(userQuestionAsMessage);
                await _unitOfWork.SaveChangesAsync(); // Lưu để có ID

                // 2. Tạo DTO thủ công thay vì dùng AutoMapper ngay lúc này
                var userQuestionMessageDto = new PrivateMessageDto
                {
                    Id = userQuestionAsMessage.Id,
                    SenderId = userAsking.Id,
                    SenderDisplayName = userAsking.DisplayName,
                    SenderAvatarUrl = userAsking.AvatarUrl,
                    ReceiverId = chatPartnerId,
                    Content = userQuestionAsMessage.Content,
                    TimestampUtc = userQuestionAsMessage.TimestampUtc,
                    IsFromAI = false,
                    IsRead = false
                };
                // -----------------------

                // 3. Gửi câu hỏi của User đến các client
                await Clients.Caller.SendAsync("ReceivePrivateMessage", userQuestionMessageDto);
                var partnerConnectionsForUserQuestion = (await _unitOfWork.UserConnectionRepository.GetByUserIdAsync(chatPartnerId)).Select(uc => uc.ConnectionId).ToList();
                if (partnerConnectionsForUserQuestion.Any())
                {
                    await Clients.Clients(partnerConnectionsForUserQuestion).SendAsync("ReceivePrivateMessage", userQuestionMessageDto);
                }

                // 4. Lấy phản hồi từ AI
                var aiResponseContent = await _aiService.GetChatCompletionAsync(userQuestion, currentUserId);
                if (string.IsNullOrWhiteSpace(aiResponseContent))
                {
                    aiResponseContent = "I'm sorry, I couldn't process that request.";
                }

                // 5. Lưu và Gửi phản hồi của AI
                // Lấy thông tin AI User (nên cache lại thay vì query DB mỗi lần)
                var aiUser = await _unitOfWork.UserRepository.GetByEmailAsync("ai@chatapp.system", tracking: false);
                Guid aiUserId = aiUser?.Id ?? Guid.Empty;
                string aiDisplayName = aiUser?.DisplayName ?? "AI Assistant";

                var aiResponseTimestamp = DateTime.UtcNow;

                // Lưu tin nhắn của AI cho người hỏi
                var aiMessageToCurrentUserDb = new PrivateMessage { Id = Guid.NewGuid(), SenderId = aiUserId, ReceiverId = currentUserId, Content = aiResponseContent, TimestampUtc = aiResponseTimestamp, IsFromAI = true, IsRead = false };
                await _unitOfWork.PrivateMessageRepository.AddAsync(aiMessageToCurrentUserDb);

                // Lưu tin nhắn của AI cho người đối thoại
                var aiMessageToPartnerDb = new PrivateMessage { Id = Guid.NewGuid(), SenderId = aiUserId, ReceiverId = chatPartnerId, Content = aiResponseContent, TimestampUtc = aiResponseTimestamp, IsFromAI = true, IsRead = false };
                await _unitOfWork.PrivateMessageRepository.AddAsync(aiMessageToPartnerDb);

                await _unitOfWork.SaveChangesAsync(); // Lưu 2 tin nhắn của AI

                // Tạo DTO cho phản hồi của AI
                var aiResponseMessageDto = new PrivateMessageDto
                {
                    Id = aiMessageToCurrentUserDb.Id,
                    SenderId = aiUserId,
                    SenderDisplayName = aiDisplayName,
                    SenderAvatarUrl = aiUser?.AvatarUrl,
                    ReceiverId = currentUserId, // Tin nhắn này là cho người hỏi
                    Content = aiResponseContent,
                    TimestampUtc = aiResponseTimestamp,
                    IsFromAI = true
                };

                // Gửi phản hồi AI cho cả 2 client
                await Clients.Caller.SendAsync("ReceivePrivateMessage", aiResponseMessageDto);
                if (partnerConnectionsForUserQuestion.Any())
                {
                    // Thay đổi ReceiverId cho partner
                    aiResponseMessageDto.ReceiverId = chatPartnerId;
                    aiResponseMessageDto.Id = aiMessageToPartnerDb.Id;
                    await Clients.Clients(partnerConnectionsForUserQuestion).SendAsync("ReceivePrivateMessage", aiResponseMessageDto);
                }

                await _unitOfWork.CommitTransactionAsync(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in AskAIInPrivateChat for user {UserId}", currentUserId);
                await _unitOfWork.RollbackTransactionAsync(transaction);
            }
        }
    }
}