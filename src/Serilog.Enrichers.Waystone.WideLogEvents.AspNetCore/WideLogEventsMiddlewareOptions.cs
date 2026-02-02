namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;

public sealed class WideLogEventsMiddlewareOptions
{
    public WideLogEventsMiddlewareOptions()
    {
        OnBeforeInvokeNext = (scope, context) =>
            scope.SetHttpRequestProperties(context.Request);

        OnAfterInvokeNext = (scope, context) =>
        {
            scope.SetOutcome(
                context.Response.StatusCode
             >= StatusCodes.Status500InternalServerError
                    ? WideLogEventOutcome.Failure
                    : WideLogEventOutcome.Success);
        };

        OnException = (scope, _, ex) =>
            scope.SetOutcome(WideLogEventOutcome.Failure, ex);

        OnFinally = (scope, context) =>
            scope.SetHttpResponseProperties(context.Response);

        Sampler = new DefaultWideLogEventsSampler();
    }

    public Action<WideLogEventScope, HttpContext>? OnBeforeInvokeNext
    {
        get;
        set;
    }

    public Action<WideLogEventScope, HttpContext>? OnAfterInvokeNext
    {
        get;
        set;
    }

    public Action<WideLogEventScope, HttpContext, Exception>? OnException
    {
        get;
        set;
    }

    public Action<WideLogEventScope, HttpContext>? OnFinally { get; set; }

    public IWideLogEventsSampler Sampler { get; set; }
}
