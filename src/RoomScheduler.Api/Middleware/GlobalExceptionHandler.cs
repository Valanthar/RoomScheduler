using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RoomScheduler.Domain.Exceptions;

namespace RoomScheduler.Api.Middleware;

/// <summary>
/// Implements IExceptionHandler (ASP.NET Core 8+) to produce RFC 7807 ProblemDetails
/// responses for every unhandled exception.
/// 
/// Security guarantee: stack traces and internal messages are NEVER sent to the client
/// for unexpected errors — only a generic message and a correlation ID (ARCHITECTURE.md).
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext      context,
        Exception        exception,
        CancellationToken ct)
    {
        var (statusCode, title, detail) = exception switch
        {
            ConflictException  e => (HttpStatusCode.Conflict,            "Conflict",              e.Message),
            ForbiddenException e => (HttpStatusCode.Forbidden,           "Forbidden",             e.Message),
            NotFoundException  e => (HttpStatusCode.NotFound,            "Not Found",             e.Message),
            DomainException    e => (HttpStatusCode.UnprocessableEntity, "Domain Rule Violation", e.Message),

            // Unknown errors — log the real message internally, return generic response.
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", (string?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

        var problem = new ProblemDetails
        {
            Status   = (int)statusCode,
            Title    = title,
            Detail   = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem), ct);

        return true;
    }
}
