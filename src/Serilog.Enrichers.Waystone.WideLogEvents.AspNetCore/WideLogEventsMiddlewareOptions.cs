namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using System;
using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;

public sealed class WideLogEventsMiddlewareOptions
{
    public WideLogEventsMiddlewareOptions()
    {
        OnBeforeInvokeNext = (scope, context) =>
            scope.SetHttpRequestProperties(context.Request);

        OnSuccess = (scope, _) =>
            scope.SetOutcome(WideLogEventOutcome.Success);

        OnException = (scope, _, ex) =>
            scope.SetOutcome(WideLogEventOutcome.Failure(ex));

        OnFinally = (scope, context) =>
            scope.SetHttpResponseProperties(context.Response);

        Sampler = new DefaultWideLogEventsSampler();
    }

    public Action<WideLogEventScope, HttpContext>? OnBeforeInvokeNext
    {
        get;
        set;
    }

    public Action<WideLogEventScope, HttpContext>? OnSuccess { get; set; }

    public Action<WideLogEventScope, HttpContext, Exception>? OnException
    {
        get;
        set;
    }

    public Action<WideLogEventScope, HttpContext>? OnFinally { get; set; }

    public IWideLogEventsSampler Sampler { get; set; }
}
