namespace Waystone.WideLogEvents;

using System;
using System.Collections.Concurrent;

public sealed class
    WideLogEventProperties : ConcurrentDictionary<string, object?>
{
    public WideLogEventProperties() =>
        this[ReservedPropertyNames.Outcome] = WideLogEventOutcome.Indeterminate;

    public WideLogEventOutcome Outcome =>
        this[ReservedPropertyNames.Outcome] as WideLogEventOutcome
     ?? WideLogEventOutcome.Indeterminate;

    public WideLogEventProperties PushProperty(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Value cannot be null or whitespace.",
                nameof(name));

        this[name] = value;

        return this;
    }
}
