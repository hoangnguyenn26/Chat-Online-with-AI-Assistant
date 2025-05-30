// src/ChatApp.Infrastructure/Services/TokenService.cs
using ChatApp.Application.Interfaces.Services;
using ChatApp.Application.Settings;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatApp.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IOptions<JwtSettings> jwtSettingsOptions, ILogger<TokenService> logger)
        {
            _jwtSettings = jwtSettingsOptions?.Value ?? throw new ArgumentNullException(nameof(jwtSettingsOptions), "JWT Settings cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                _logger.LogCritical("JWT Key is not configured in JwtSettings.");
                throw new InvalidOperationException("JWT Key is not configured.");
            }
            // Recommended length for HS256 is at least 32 bytes (256 bits)
            if (Encoding.UTF8.GetBytes(_jwtSettings.Key).Length < 32)
            {
                _logger.LogCritical("JWT Key is too short. It must be a strong key of at least 32 bytes (256 bits).");
                throw new InvalidOperationException("JWT Key is too short. Please use a stronger key.");
            }
            if (string.IsNullOrEmpty(_jwtSettings.Issuer))
            {
                _logger.LogWarning("JWT Issuer is not configured in JwtSettings.");
                // Consider throwing an exception if Issuer is strictly required
            }
            if (string.IsNullOrEmpty(_jwtSettings.Audience))
            {
                _logger.LogWarning("JWT Audience is not configured in JwtSettings.");
                // Consider throwing an exception if Audience is strictly required
            }
            if (_jwtSettings.DurationInMinutes <= 0)
            {
                _logger.LogWarning("JWT ExpiryMinutes is not configured atau invalid in JwtSettings. Defaulting to 60 minutes for safety.");
                _jwtSettings.DurationInMinutes = 60; // Default expiry if not set or invalid
            }
        }

        public string GenerateJwtToken(User user, IEnumerable<string>? roles = null)
        {
            if (user == null)
            {
                _logger.LogError("Cannot generate JWT token for a null user.");
                throw new ArgumentNullException(nameof(user));
            }

            _logger.LogInformation("Generating JWT token for User {UserId} - {UserEmail}", user.Id, user.Email);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
                new Claim("uid", user.Id.ToString()),
                new Claim("avatar", user.AvatarUrl ?? string.Empty)
            };

            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
                _logger.LogDebug("Added roles to JWT for User {UserId}: {Roles}", user.Id, string.Join(",", roles));
            }
            else
            {
                _logger.LogDebug("No roles provided or roles list is empty for User {UserId}", user.Id);
            }


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            _logger.LogInformation("JWT token generated successfully for User {UserId}. Token Expiry: {ExpiryTime}", user.Id, tokenDescriptor.Expires);
            return tokenString;
        }
    }
}