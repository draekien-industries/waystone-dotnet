namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using System;
using System.Diagnostics;
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
        ILogger<WideLogEventsMiddleware> logger)
    {
        using WideLogEventScope scope = WideLogEventContext.BeginScope();

        string operationName =
            $"{context.Request.Method} {context.Request.Path}"
               .Replace("/r", string.Empty, StringComparison.Ordinal)
               .Replace("/n", string.Empty, StringComparison.Ordinal);

        long startTime = Stopwatch.GetTimestamp();

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
            options.OnFinally?.Invoke(scope, context);

            TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);

            LogLevel logLevel = options.Sampler.GetLogLevel(scope);

            bool shouldLog = logger.IsEnabled(logLevel)
             && options.Sampler.ShouldSample(scope);

            if (shouldLog)
            {
                logger.Log(
                    logLevel,
                    "{OperationName} completed in {ElapsedMs}ms with status code {StatusCode}",
                    operationName,
                    elapsed.TotalMilliseconds,
                    context.Response.StatusCode);
            }
        }
    }
}
