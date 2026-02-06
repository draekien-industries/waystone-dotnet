namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class WideLogEventsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ILogger<WideLogEventsMiddleware> logger)
    {
        using WideLogEventScope scope = WideLogEventContext.BeginScope();

        await next(context);
    }
}
