namespace Waystone.WideLogEvents;

using System;
using System.Collections.Concurrent;

public sealed class
    WideLogEventProperties : ConcurrentDictionary<string, object?>
{
    public WideLogEventProperties() => Outcome = WideLogEventOutcome.Success;

    public WideLogEventOutcome Outcome
    {
        get =>
            (WideLogEventOutcome)(this[ReservedPropertyNames.Outcome]
             ?? WideLogEventOutcome.Indeterminate);
        set => this[ReservedPropertyNames.Outcome] = value;
    }

    public Exception? Exception { get; set; }

    public WideLogEventProperties PushProperty(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value cannot be null or whitespace.",
                nameof(name));
        }

        if (ReservedPropertyNames.IsReserved(name))
        {
            throw new ArgumentException(
                $"Value {name} is reserved.",
                nameof(name));
        }

        this[name] = value;

        return this;
    }
}
