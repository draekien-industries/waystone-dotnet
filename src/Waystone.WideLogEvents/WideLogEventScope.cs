namespace Waystone.WideLogEvents;

using System;
using Monads.Options;

public readonly struct WideLogEventScope : IDisposable
{
    private readonly Option<WideLogEventProperties> _previous;

    private static Option<WideLogEventProperties> TrackedProperties =>
        WideLogEventContext.ScopedProperties.Value;

    public WideLogEventScope()
    {
        _previous = TrackedProperties;

        WideLogEventContext.ScopedProperties.Value =
            Option.Some(new WideLogEventProperties());
    }

    private WideLogEventScope(Option<WideLogEventProperties> properties)
    {
        _previous = TrackedProperties;
        WideLogEventContext.ScopedProperties.Value = properties;
    }

    public static WideLogEventScope Resume(
        Option<WideLogEventProperties> properties) =>
        new(properties);

    public void Dispose()
    {
        WideLogEventContext.ScopedProperties.Value = _previous;
    }

    public WideLogEventScope PushProperty(string name, object? value)
    {
        TrackedProperties.Inspect(scope => scope.PushProperty(name, value));

        return this;
    }

    public WideLogEventScope SetOutcome(WideLogEventOutcome outcome) =>
        PushProperty(ReservedPropertyNames.Outcome, outcome);

    public WideLogEventOutcome Outcome =>
        TrackedProperties.MapOr(
            WideLogEventOutcome.Indeterminate,
            scope => scope.Outcome);
}
