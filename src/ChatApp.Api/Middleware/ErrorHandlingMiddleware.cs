namespace ChatApp.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
        }
        public async Task InvokeAsync(HttpContext context)
        {
            await Task.CompletedTask;
        }
    }
}
