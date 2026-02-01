namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using System;
using System.Threading.Tasks;
using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class WideLogEventsMiddleware(
    RequestDelegate next,
    WideLogEventsMiddlewareOptions options)
{
    public async Task InvokeAsync(
        HttpContext context,
        ILoggerFactory loggerFactory)
    {
        using WideLogEventScope scope = WideLogEventContext.BeginScope();

        ILogger logger = loggerFactory.CreateLogger(
            $"{context.Request.Method} {context.Request.Path}");

        try
        {
            options.OnBeforeInvokeNext?.Invoke(scope, context);

            await next(context);

            options.OnSuccess?.Invoke(scope, context);
        }
        catch (Exception ex)
        {
            options.OnException?.Invoke(scope, context, ex);

            throw;
        }
        finally
        {
            options.OnPostInvokeNext?.Invoke(scope, context);

            LogLevel logLevel = options.Sampler.GetLogLevel(scope);

            if (logger.IsEnabled(logLevel)
             && options.Sampler.ShouldSample(scope))
            {
                logger.Log(logLevel, "Request completed");
            }
        }
    }
}
