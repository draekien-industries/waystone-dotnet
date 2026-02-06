namespace Waystone.WideLogEvents;

using System;
using System.Collections.Concurrent;

public readonly struct WideLogEventScope : IDisposable
{
    private readonly ConcurrentDictionary<string, object?>? _previous;

    public WideLogEventScope()
    {
        _previous = WideLogEventContext.ScopedProperties.Value;

        WideLogEventContext.ScopedProperties.Value =
            new ConcurrentDictionary<string, object?>();
    }

    public void Dispose()
    {
        WideLogEventContext.ScopedProperties.Value = _previous;
    }

    public WideLogEventScope PushProperty(string name, object? value)
    {
        WideLogEventContext.PushProperty(name, value);

        return this;
    }
}
