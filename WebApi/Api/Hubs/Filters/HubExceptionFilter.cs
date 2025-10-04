using Microsoft.AspNetCore.SignalR;

namespace WebApi.Api.Hubs.Filters;

public class HubExceptionFilter : IHubFilter
{
    private readonly ILogger<HubExceptionFilter> _logger;

    public HubExceptionFilter(ILogger<HubExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Hub: '{invocationContext.Hub.GetType().Name}'," +
                $"method: '{invocationContext.HubMethodName}'");

            throw new HubException("An internal server error occurred");
        }
    }
}

