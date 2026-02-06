namespace Waystone.WideLogEvents;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public sealed class WideLogEventContext
{
    internal static readonly
        AsyncLocal<ConcurrentDictionary<string, object?>?> ScopedProperties =
            new();

    public static WideLogEventScope BeginScope() => new();

    public static IReadOnlyDictionary<string, object?> GetScopedProperties() =>
        ScopedProperties.Value
     ?? (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?>();

    public static void PushProperty(string name, object? value)
    {
        if (ScopedProperties.Value is null)
        {
            throw new InvalidOperationException(
                "'WideLogEventContext' has not been initialized. Invoke 'WideLogEventContext.BeginScope()' before beginning to push properties");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value cannot be null or whitespace.",
                nameof(name));
        }

        ScopedProperties.Value[name] = value;
    }
}
