using AutoMapper; // Cần cho IMapper
using ChatApp.Application.Dtos.Users;
using ChatApp.Application.Interfaces; // IUnitOfWork
using ChatApp.Application.Interfaces.Services;
using ChatApp.Domain.Entities;
using LinqKit;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserDto?> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching profile for current user {UserId}", userId);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken, tracking: false);

            if (user == null)
            {
                _logger.LogWarning("Current user {UserId} not found.", userId);
                return null;
            }
            var userDto = _mapper.Map<UserDto>(user);
            userDto.IsOnline = await _unitOfWork.UserConnectionRepository.UserHasConnectionsAsync(userId, cancellationToken);
            return userDto;
        }

        public async Task<UserDto?> GetUserProfileByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching profile for user {UserId}", userId);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken, tracking: false);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found.", userId);
                return null;
            }
            var userDto = _mapper.Map<UserDto>(user);
            userDto.IsOnline = await _unitOfWork.UserConnectionRepository.UserHasConnectionsAsync(userId, cancellationToken);
            return userDto;
        }


        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(
            Guid currentUserId,
            string? searchQuery = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all users. CurrentUserId: {CurrentUserId}, Search: {SearchQuery}, Page: {Page}", currentUserId, searchQuery ?? "None", page);

            var predicate = PredicateBuilder.New<User>(u => u.Id != currentUserId);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var term = searchQuery.Trim().ToLower();
                predicate = predicate.And(u =>
                    (u.DisplayName != null && u.DisplayName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term))
                );
            }

            var users = await _unitOfWork.UserRepository.GetUsersAsync(
                filter: predicate,
                orderBy: q => q.OrderBy(u => u.DisplayName),
                page: page,
                pageSize: pageSize,
                tracking: false,
                cancellationToken: cancellationToken
            );

            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users).ToList();

            // Cập nhật trạng thái online cho từng DTO
            foreach (var dto in userDtos)
            {
                dto.IsOnline = await _unitOfWork.UserConnectionRepository.UserHasConnectionsAsync(dto.Id, cancellationToken);
            }

            return userDtos;
        }

        public async Task<User> FindOrCreateUserFromOAuthAsync(
            string providerName,
            string externalId,
            string email,
            string displayName,
            string? avatarUrl,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Finding or creating user from OAuth. Provider: {Provider}, ExternalId: {ExternalId}, Email: {Email}", providerName, externalId, email);

            var user = await _unitOfWork.UserRepository.GetByExternalIdAsync(providerName, externalId, cancellationToken, tracking: true);

            if (user == null)
            {
                _logger.LogInformation("User not found by ExternalId. Checking by Email: {Email}", email);
                user = await _unitOfWork.UserRepository.GetByEmailAsync(email, cancellationToken, tracking: true);

                if (user != null) // Tìm thấy bằng Email, user này có thể đã đăng ký bằng cách khác hoặc OAuth provider khác
                {
                    _logger.LogInformation("User found by Email {Email}. Linking with Provider: {Provider}, ExternalId: {ExternalId}", email, providerName, externalId);
                    if (string.IsNullOrEmpty(user.ExternalId) || string.IsNullOrEmpty(user.ProviderName))
                    {
                        user.ExternalId = externalId;
                        user.ProviderName = providerName;
                        // Cập nhật DisplayName và AvatarUrl nếu chúng tốt hơn hoặc chưa có
                        user.DisplayName = !string.IsNullOrWhiteSpace(displayName) ? displayName : user.DisplayName;
                        user.AvatarUrl = !string.IsNullOrWhiteSpace(avatarUrl) ? avatarUrl : user.AvatarUrl;
                        // UpdatedAtUtc sẽ tự cập nhật
                    }
                    else if (user.ProviderName != providerName)
                    {
                        _logger.LogWarning("Email {Email} is already associated with provider {ExistingProvider}. Cannot link new provider {NewProvider}.", email, user.ProviderName, providerName);
                        throw new ValidationException($"This email is already associated with another login method ({user.ProviderName}). Please log in using that method or use a different email.");
                    }
                    // Nếu user.ProviderName == providerName nhưng ExternalId khác -> Lỗi logic hoặc dữ liệu không nhất quán.
                }
                else // User hoàn toàn mới
                {
                    _logger.LogInformation("Creating new user. Email: {Email}, Provider: {Provider}", email, providerName);
                    user = new User
                    {
                        ExternalId = externalId,
                        Email = email,
                        DisplayName = displayName,
                        AvatarUrl = avatarUrl,
                        ProviderName = providerName,
                        IsActive = true,
                    };
                    await _unitOfWork.UserRepository.AddAsync(user, cancellationToken);
                }
            }
            else // User đã tồn tại với ExternalId này, cập nhật thông tin nếu cần
            {
                _logger.LogInformation("Existing user found by ExternalId {ExternalId} for Provider {Provider}. Updating info if changed.", externalId, providerName);
                bool changed = false;
                if (user.DisplayName != displayName && !string.IsNullOrWhiteSpace(displayName)) { user.DisplayName = displayName; changed = true; }
                if (user.Email != email && !string.IsNullOrWhiteSpace(email)) { user.Email = email; changed = true; }
                if (user.AvatarUrl != avatarUrl && !string.IsNullOrWhiteSpace(avatarUrl)) { user.AvatarUrl = avatarUrl; changed = true; }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task UpdateUserPresenceAsync(Guid userId, bool isOnline, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Updating presence for User {UserId}. IsOnline: {IsOnline}", userId, isOnline);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken, tracking: true);
            if (user != null)
            {
                if (isOnline)
                {
                }
                else
                {
                    user.LastSeenUtc = DateTime.UtcNow;
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found during presence update.", userId);
            }
        }
    }
}