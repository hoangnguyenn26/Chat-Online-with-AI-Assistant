using System.Net;
using System.Text.Json;

namespace ChatApp.Api.Middleware
{
    public class ErrorResponse
    {
        public string Error { get; set; } = default!;
        public string? StackTrace { get; set; }
        public string? Path { get; set; }
        public int StatusCode { get; set; }
    }

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while processing request {Path}", context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new ErrorResponse
                {
                    Error = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.",
                    StackTrace = _env.IsDevelopment() ? ex.StackTrace : null,
                    Path = _env.IsDevelopment() ? context.Request.Path : null,
                    StatusCode = context.Response.StatusCode
                };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}