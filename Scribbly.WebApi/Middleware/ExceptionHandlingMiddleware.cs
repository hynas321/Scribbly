using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Persistence;

namespace Scribbly.WebApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (GameStateConflictException) when (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "The game changed during this operation. Refresh and try again."
            }, cancellationToken: context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception occurred for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            ProblemDetails problem = new()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error"
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                problem,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                cancellationToken: context.RequestAborted);
        }
    }
}
