using System.Net;
using System.Text.Json;
using TaskManagementAPI.Contracts;
using Serilog;

namespace TaskManagementAPI.Middleware;

/// <summary>
/// Catches every unhandled exception that bubbles up the pipeline and turns it
/// into a uniform <see cref="ErrorResponse"/> JSON payload.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // ── Map known exception types to HTTP status codes ─────────────────
        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound, ex.Message),
            InvalidOperationException   => (HttpStatusCode.Conflict, ex.Message),
            ArgumentException           => (HttpStatusCode.BadRequest, ex.Message),
            _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        // Log everything; internal errors get the full stack trace
        if (statusCode == HttpStatusCode.InternalServerError)
            Log.Error(ex, "Unhandled exception");
        else
            Log.Warning(ex, "Handled exception → {StatusCode}", (int)statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsJsonAsync(response, options);
    }
}

// ─── Extension method so Program.cs stays clean ─────────────────────────────
public static class GlobalExceptionHandlerExtensions
{
    /// <summary>Registers the <see cref="GlobalExceptionHandlerMiddleware"/>.</summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
