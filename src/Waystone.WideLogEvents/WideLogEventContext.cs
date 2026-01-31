namespace Waystone.WideLogEvents;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public sealed class WideLogEventContext :
    AsyncLocalScoped<ConcurrentDictionary<string, object?>>
{
    public static IDisposable BeginScope() =>
        BeginScope(new ConcurrentDictionary<string, object?>());

    public static void PushProperty(string name, object? value) =>
        ScopedValue.Inspect(scope =>
            scope.AddOrUpdate(name, value, (_, _) => value));

    public static IReadOnlyDictionary<string, object?> GetProperties() =>
        ScopedValue.Match(some => some, () => []);
}
