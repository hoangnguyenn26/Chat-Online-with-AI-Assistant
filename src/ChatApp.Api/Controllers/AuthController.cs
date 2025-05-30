using ChatApp.Application.Interfaces;
using ChatApp.Application.Interfaces.Services;
using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ITokenService tokenService,
            IUserService userService,
            IUnitOfWork unitOfWork,
            ILogger<AuthController> logger)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork)); // Ensure it's injected
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle(string? returnUrl = "/")
        {
            _logger.LogInformation("Initiating Google login. Return URL (client-side): {ReturnUrl}", returnUrl);
            var properties = new AuthenticationProperties
            {

            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received Google callback.");
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                _logger.LogWarning("Google authentication failed or principal is null.");
                return Redirect($"http://localhost:4200/auth/login-failed?error={Uri.EscapeDataString("Google authentication failed.")}");
            }

            _logger.LogInformation("Google authentication successful.");

            var claims = authenticateResult.Principal.Claims.ToList();
            var emailClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var googleIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var givenNameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            var surnameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            var pictureClaim = claims.FirstOrDefault(c => c.Type == "urn:google:picture") ?? claims.FirstOrDefault(c => c.Type == "picture");

            if (emailClaim == null || googleIdClaim == null)
            {
                _logger.LogWarning("Essential claims (Email or GoogleId) not found in Google principal.");
                return Redirect($"http://localhost:4200/auth/login-failed?error={Uri.EscapeDataString("Missing essential claims from Google.")}");
            }

            var userEmail = emailClaim.Value;
            var googleUserId = googleIdClaim.Value;
            var displayName = nameClaim?.Value ?? $"{givenNameClaim?.Value} {surnameClaim?.Value}".Trim();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = userEmail.Split('@')[0];
            var avatarUrl = pictureClaim?.Value;

            User user;
            try
            {
                user = await _userService.FindOrCreateUserFromOAuthAsync(
                    providerName: "Google",
                    externalId: googleUserId,
                    email: userEmail,
                    displayName: displayName,
                    avatarUrl: avatarUrl,
                    cancellationToken
                );
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error during FindOrCreateUserFromOAuthAsync in Google callback.");
                return Redirect($"http://localhost:4200/auth/login-failed?error={Uri.EscapeDataString(ex.Message)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FindOrCreateUserFromOAuthAsync during Google callback.");
                return Redirect($"http://localhost:4200/auth/login-failed?error={Uri.EscapeDataString("Error processing user information.")}");
            }

            _logger.LogInformation("User {UserId} ({Email}) processed from Google callback via UserService.", user.Id, user.Email);
            IEnumerable<string>? appUserRoles = null;

            string appJwtToken = _tokenService.GenerateJwtToken(user, appUserRoles);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var clientCallbackUrl = $"http://localhost:4200/auth-callback?token={Uri.EscapeDataString(appJwtToken)}";
            _logger.LogInformation("Redirecting to client with token: {ClientCallbackUrl}", clientCallbackUrl);
            return Redirect(clientCallbackUrl);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("/auth/me: User ID not found or invalid in token claims.");
                return Unauthorized(new { message = "User identifier not found or invalid in token." });
            }

            var userProfileDto = await _userService.GetCurrentUserProfileAsync(userId, cancellationToken);
            if (userProfileDto == null)
            {
                _logger.LogWarning("/auth/me: User profile DTO not found for UserId {UserId} despite valid token.", userId);
                return Unauthorized(new { message = "User profile not found." });
            }
            return Ok(userProfileDto);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} requesting logout.", userId ?? "Unknown");

            if (User.Identity != null && User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("Signed out from intermediate cookie for User {UserId}.", userId);
            }
            _logger.LogInformation("Logout processed for User {UserId}. Client should clear token.", userId);
            return Ok(new { message = "Logged out successfully. Please clear your token on the client side." });
        }
    }
}