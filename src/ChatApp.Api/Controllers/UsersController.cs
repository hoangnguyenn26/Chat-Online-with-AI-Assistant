using ChatApp.Application.Dtos.Users;
using ChatApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("User identifier not found or invalid in token.");
            }
            return userId;
        }

        // GET: api/users
        // Ví dụ: api/users?searchQuery=john&page=1&pageSize=10
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
            [FromQuery] string? searchQuery = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Guid currentUserId = GetCurrentUserId();
                _logger.LogInformation("User {CurrentUserId} requesting user list. Search: '{SearchQuery}', Page: {Page}, PageSize: {PageSize}",
                    currentUserId, searchQuery ?? "None", page, pageSize);

                var users = await _userService.GetAllUsersAsync(
                    currentUserId,
                    searchQuery,
                    page,
                    pageSize,
                    cancellationToken
                );
                return Ok(users);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in GetUsers.");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching users.");
            }
        }

        // GET: api/users/{userId}
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                Guid currentUserId = GetCurrentUserId();
                _logger.LogInformation("User {CurrentUserId} requesting profile for User {TargetUserId}", currentUserId, userId);

                UserDto? userDto;
                userDto = await _userService.GetUserProfileByIdAsync(userId, cancellationToken);
                // }


                if (userDto == null)
                {
                    _logger.LogWarning("User with Id {TargetUserId} not found.", userId);
                    return NotFound(new { message = $"User with Id '{userId}' not found." });
                }
                return Ok(userDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in GetUserById.");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user by Id {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching user details.");
            }
        }
    }
}