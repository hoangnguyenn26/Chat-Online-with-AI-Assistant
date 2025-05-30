using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatApp.Api.SignalR
{
    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public virtual string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}