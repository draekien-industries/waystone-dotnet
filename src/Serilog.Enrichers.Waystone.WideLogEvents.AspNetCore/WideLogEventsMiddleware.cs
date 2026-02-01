namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class WideLogEventsMiddleware(
    RequestDelegate next,
    ILogger<WideLogEventsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using WideLogEventScope scope = WideLogEventContext.BeginScope();

        long startedAt = Stopwatch.GetTimestamp();

        try
        {
            scope.PushProperty(
                ReservedPropertyNames.HttpRequest,
                new
                {
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.PathBase,
                    context.Request.Scheme,
                    context.Request.Host,
                    context.Request.ContentType,
                    context.Request.ContentLength,
                    context.Request.Protocol,
                    context.Request.Query,
                });

            await next(context);
            scope.SetOutcome(WideLogEventOutcome.Success());
        }
        catch (Exception ex)
        {
            scope.SetOutcome(WideLogEventOutcome.Failure(ex));

            throw;
        }
        finally
        {
            long completedAt = Stopwatch.GetTimestamp();
            long durationMs = completedAt - startedAt;

            LogLevel logLevel = scope.Outcome switch
            {
                SuccessWideLogEventOutcome => LogLevel.Information,
                FailureWideLogEventOutcome => LogLevel.Error,
                IndeterminateWideLogEventOutcome => LogLevel.Warning,
                var _ => LogLevel.None,
            };

            if (logger.IsEnabled(logLevel))
            {
                logger.Log(
                    logLevel,
                    "Request completed in {DurationMs} ms.",
                    durationMs);
            }
        }
    }
}
