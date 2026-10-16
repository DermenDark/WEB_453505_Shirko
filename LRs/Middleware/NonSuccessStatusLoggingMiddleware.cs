using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace LRs.Middleware;
public sealed class NonSuccessStatusLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NonSuccessStatusLoggingMiddleware> _logger;
    public NonSuccessStatusLoggingMiddleware(
        RequestDelegate next,
        ILogger<NonSuccessStatusLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            var status = context.Response.StatusCode;
            if (status < 200 || status >= 300)
            {
                var path = context.Request.Path + context.Request.QueryString;
                _logger.LogInformation(
                    "---> request {RequestPath} returns {StatusCode}",
                    path.ToString(), status);
            }
        }
    }
}