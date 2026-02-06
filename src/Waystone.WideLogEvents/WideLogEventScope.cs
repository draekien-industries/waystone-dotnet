namespace Waystone.WideLogEvents;

using System;
using System.Collections.Concurrent;

public readonly struct WideLogEventScope : IDisposable
{
    private readonly ConcurrentDictionary<string, object?>? _previousProperties;

    public WideLogEventScope()
    {
        _previousProperties = WideLogEventContext.ScopedProperties.Value;

        WideLogEventContext.ScopedProperties.Value =
            new ConcurrentDictionary<string, object?>();
    }

    public void Dispose()
    {
        WideLogEventContext.ScopedProperties.Value = _previousProperties;
    }

    public WideLogEventScope PushProperty(string name, object? value)
    {
        WideLogEventContext.PushProperty(name, value);

        return this;
    }
}
