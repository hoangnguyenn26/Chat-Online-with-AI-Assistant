using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence.DbContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        // private readonly ITokenService _tokenService;

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger /*, ITokenService tokenService */)
        {
            _context = context;
            _logger = logger;
            // _tokenService = tokenService;
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle(string? returnUrl = "http://localhost:4200/auth-callback")
        {
            _logger.LogInformation("Initiating Google login. Client return URL: {ReturnUrl}", returnUrl);
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(ProcessGoogleResponse)), // Action sẽ xử lý sau khi Google middleware tạo cookie
                Items = { { "LoginProvider", "Google" }, { "ReturnUrl", returnUrl! } }
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Route này không phải là callback path trực tiếp của Google,
        // mà là nơi CookieAuth middleware redirect tới SAU KHI Google middleware đã xử lý /signin-google
        [HttpGet("process-google-response")]
        public async Task<IActionResult> ProcessGoogleResponse()
        {
            _logger.LogInformation("Processing Google login response after external cookie authentication.");
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                _logger.LogWarning("External authentication (cookie) failed or principal is null.");
                string? clientReturnUrl = authenticateResult.Properties?.Items["ReturnUrl"] ?? "http://localhost:4200/auth/login-failed";
                return Redirect($"{clientReturnUrl}?error=google_auth_failed_after_cookie");
            }

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
                string? clientReturnUrl = authenticateResult.Properties?.Items["ReturnUrl"] ?? "http://localhost:4200/auth/login-failed";
                return Redirect($"{clientReturnUrl}?error=missing_claims");
            }

            var userEmail = emailClaim.Value;
            var googleUserId = googleIdClaim.Value;
            var displayName = nameClaim?.Value ?? $"{givenNameClaim?.Value} {surnameClaim?.Value}".Trim();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = userEmail.Split('@')[0];
            var avatarUrl = pictureClaim?.Value;

            _logger.LogInformation("User claims extracted: Email={Email}, GoogleId={GoogleId}, Name={DisplayName}", userEmail, googleUserId, displayName);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ExternalId == googleUserId && u.ProviderName == "Google");

            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(user.ExternalId) || string.IsNullOrEmpty(user.ProviderName))
                    {
                        user.ExternalId = googleUserId;
                        user.ProviderName = "Google";
                        user.DisplayName = !string.IsNullOrWhiteSpace(displayName) ? displayName : user.DisplayName;
                        user.AvatarUrl = !string.IsNullOrWhiteSpace(avatarUrl) ? avatarUrl : user.AvatarUrl;
                        user.UpdatedAtUtc = DateTime.UtcNow;
                        _logger.LogInformation("Linking Google account to existing user {UserId} with email {Email}.", user.Id, userEmail);
                    }
                    else if (user.ProviderName != "Google")
                    {
                        _logger.LogWarning("Email {Email} already associated with another provider: {Provider}. Google login for {GoogleId} blocked.", userEmail, user.ProviderName, googleUserId);
                        string? clientReturnUrl = authenticateResult.Properties?.Items["ReturnUrl"] ?? "http://localhost:4200/auth/login-failed";
                        return Redirect($"{clientReturnUrl}?error=email_conflict_provider&provider={user.ProviderName}");
                    }
                }
                else
                {
                    user = new User
                    {
                        ExternalId = googleUserId,
                        Email = userEmail,
                        DisplayName = displayName,
                        AvatarUrl = avatarUrl,
                        ProviderName = "Google",
                        IsActive = true,
                    };
                    _context.Users.Add(user);
                    _logger.LogInformation("Creating new user from Google login: {Email}", userEmail);
                }
            }
            else
            {
                bool changed = false;
                if (user.DisplayName != displayName && !string.IsNullOrWhiteSpace(displayName)) { user.DisplayName = displayName; changed = true; }
                if (user.AvatarUrl != avatarUrl && !string.IsNullOrWhiteSpace(avatarUrl)) { user.AvatarUrl = avatarUrl; changed = true; }
                if (changed) user.UpdatedAtUtc = DateTime.UtcNow;
                _logger.LogInformation("Existing Google user found: {Email}. Info updated: {Changed}", userEmail, changed);
            }

            await _context.SaveChangesAsync();

            // --- TẠO JWT TOKEN CỦA ỨNG DỤNG BẠN ---
            // string appJwtToken = _tokenService.GenerateJwtToken(user); // Giả sử bạn đã có _tokenService
            _logger.LogInformation("User {UserId} ({Email}) processed. JWT generation (TODO).", user.Id, user.Email);


            // Đăng xuất khỏi cookie scheme trung gian của Google
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            string? finalClientReturnUrl = authenticateResult.Properties?.Items["ReturnUrl"] ?? "http://localhost:4200/auth-callback";

            // Khi có JWT, sẽ là: return Redirect($"{finalClientReturnUrl}?token={appJwtToken}");
            // Tạm thời redirect với thông tin user để Angular xử lý (hoặc có thể chỉ là một tham số success=true)
            _logger.LogInformation("Redirecting to client: {ClientUrl}", finalClientReturnUrl);
            return Redirect($"{finalClientReturnUrl}?userId={user.Id}&email={user.Email}&displayName={Uri.EscapeDataString(user.DisplayName)}&avatarUrl={Uri.EscapeDataString(avatarUrl ?? string.Empty)}");
        }

        // [HttpPost("logout")] 
        // public async Task<IActionResult> Logout()
        // {
        //     await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //     _logger.LogInformation("User logged out.");
        //     return Ok(new { message = "Logged out successfully" });
        // }
    }
}