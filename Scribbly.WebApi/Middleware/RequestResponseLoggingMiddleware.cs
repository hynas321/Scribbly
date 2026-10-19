using Microsoft.AspNetCore.Mvc.Controllers;
using System.Diagnostics;

namespace Scribbly.WebApi.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        Endpoint? endpoint = context.GetEndpoint();
        ControllerActionDescriptor? descriptor =
            endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (descriptor is null)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        string controller = descriptor.ControllerName;
        string action = descriptor.ActionName;
        string path = context.Request.Path.ToString();

        _logger.LogInformation(
            "Started {Method} {Path} ({Controller}/{Action})",
            context.Request.Method,
            path,
            controller,
            action);

        try
        {
            await _next(context);
            stopwatch.Stop();
            int statusCode = context.Response.StatusCode;
            LogLevel level = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(
                level,
                "Completed {StatusCode} {Method} {Path} ({Controller}/{Action}) in {Elapsed} ms",
                statusCode,
                context.Request.Method,
                path,
                controller,
                action,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                exception,
                "Failed {Method} {Path} ({Controller}/{Action}) after {Elapsed} ms",
                context.Request.Method,
                path,
                controller,
                action,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
