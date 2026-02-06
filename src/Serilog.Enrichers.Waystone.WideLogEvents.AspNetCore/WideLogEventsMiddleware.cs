namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;

public class WideLogEventsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using WideLogEventScope scope = WideLogEventContext.BeginScope();

        await next(context);
    }
}
