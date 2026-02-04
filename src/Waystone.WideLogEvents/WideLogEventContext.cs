namespace Waystone.WideLogEvents;

using System;
using System.Collections.Generic;
using System.Threading;

public sealed class WideLogEventContext
{
    internal static readonly
        AsyncLocal<WideLogEventProperties?> ScopedProperties = new();

    public static WideLogEventScope BeginScope() => new();

    public static IReadOnlyDictionary<string, object?> GetScopedProperties() =>
        ScopedProperties.Value ?? [];

    public static void PushProperty(string name, object? value)
    {
        if (ScopedProperties.Value is null)
        {
            throw new InvalidOperationException(
                "'WideLogEventContext' has not been initialized. Invoke 'WideLogEventContext.BeginScope()' before beginning to push properties");
        }

        ScopedProperties.Value.PushProperty(name, value);
    }

    public static void AppendEvent()
    { }
}
