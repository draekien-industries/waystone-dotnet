namespace Waystone.WideLogEvents;

using System.Collections.Generic;
using System.Threading;

public sealed class WideLogEventContext
{
    internal static readonly
        AsyncLocal<WideLogEventProperties?> ScopedProperties = new();

    public static WideLogEventScope BeginScope() => new();

    public static IReadOnlyDictionary<string, object?> GetScopedProperties() =>
        ScopedProperties.Value ?? [];
}
