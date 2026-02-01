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

    private WideLogEventScope(WideLogEventProperties properties)
    {
        _previous = TrackedProperties;
        WideLogEventContext.ScopedProperties.Value = properties;
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

    public WideLogEventScope SetOutcome(WideLogEventOutcome outcome) =>
        PushProperty(ReservedPropertyNames.Outcome, outcome);

    public WideLogEventOutcome Outcome => TrackedProperties.Outcome;
}
