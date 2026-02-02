namespace Waystone.WideLogEvents;

using System;

public readonly struct WideLogEventScope : IDisposable
{
    private readonly WideLogEventProperties? _previous;

    private static WideLogEventProperties TrackedProperties =>
        WideLogEventContext.ScopedProperties.Value ?? [];

    public WideLogEventScope()
    {
        _previous = TrackedProperties;

        WideLogEventContext.ScopedProperties.Value =
            new WideLogEventProperties();
    }

    public void Dispose()
    {
        WideLogEventContext.ScopedProperties.Value = _previous;
    }

    public WideLogEventScope PushProperty(string name, object? value)
    {
        TrackedProperties.PushProperty(name, value);

        return this;
    }

    public WideLogEventScope SetOutcome(
        WideLogEventOutcome outcome,
        Exception? exception = null)
    {
        TrackedProperties.Outcome = outcome;
        TrackedProperties.Exception = exception;

        return this;
    }

    public WideLogEventOutcome Outcome => TrackedProperties.Outcome;
    public Exception? Exception => TrackedProperties.Exception;
}
