using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatforms.Back.Middlewares;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler, IMiddleware
{
    readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try { await next(context); }
        catch (Exception e)
        {
            await TryHandleAsync(context, e);
        }
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken = default)
    {
        var res = context.Response;
        var (status, title) = exception switch
        {
            NotImplementedException => (StatusCodes.Status501NotImplemented, "Not Implemented"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
        ProblemDetails problem = new()
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Status = res.StatusCode = status,
            Title = title,
            Detail = exception.Message
        };

        await res.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}