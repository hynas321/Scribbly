using Microsoft.AspNetCore.SignalR;

namespace WebApi.Api.Hubs.Filters;

public class HubExceptionFilter : IHubFilter
{
    private readonly ILogger<HubExceptionFilter> _logger;

    public HubExceptionFilter(ILogger<HubExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (HubException hubException)
        {
            _logger.LogWarning(hubException,
                "Hub validation or client-related error in {Hub}.{Method}: {Message}",
                invocationContext.Hub.GetType().Name,
                invocationContext.HubMethodName,
                hubException.Message);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in Hub {Hub}.{Method}",
                invocationContext.Hub.GetType().Name,
                invocationContext.HubMethodName);

            throw new HubException("An internal server error occurred");
        }
    }
}
