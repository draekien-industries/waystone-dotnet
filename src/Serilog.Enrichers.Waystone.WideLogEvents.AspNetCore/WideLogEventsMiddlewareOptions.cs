namespace Serilog.Enrichers.Waystone.WideLogEvents.AspNetCore;

using System;
using global::Waystone.WideLogEvents;
using Microsoft.AspNetCore.Http;

public sealed class WideLogEventsMiddlewareOptions
{
    public Action<WideLogEventScope, HttpContext>? OnBeforeInvokeNext
    {
        get;
        set;
    } = (scope, context) => scope.PushProperty(
        ReservedPropertyNames.HttpRequest,
        new
        {
            context.Request.Method,
            Path = context.Request.Path.ToString(),
            PathBase = context.Request.PathBase.ToString(),
            context.Request.Scheme,
            Host = context.Request.Host.ToString(),
            context.Request.ContentType,
            context.Request.ContentLength,
            context.Request.Protocol,
            context.Request.Query,
        });

    public Action<WideLogEventScope, HttpContext>? OnSuccess { get; set; } =
        (scope, _) => scope.SetOutcome(WideLogEventOutcome.Success());

    public Action<WideLogEventScope, HttpContext, Exception>? OnException
    {
        get;
        set;
    } = (scope, _, ex) =>
        scope.SetOutcome(WideLogEventOutcome.Failure(ex));

    public Action<WideLogEventScope, HttpContext>? OnPostInvokeNext
    {
        get;
        set;
    }

    public IWideLogEventsSampler Sampler { get; set; } =
        new DefaultWideLogEventsSampler();
}
