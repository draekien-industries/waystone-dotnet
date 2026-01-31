namespace Waystone.WideLogEvents;

using System;
using System.Collections.Concurrent;

public sealed class WideLogEventContext :
    AsyncLocalScoped<ConcurrentDictionary<string, object?>>
{
    public static IDisposable BeginScope() =>
        BeginScope(new ConcurrentDictionary<string, object?>());

    public void PushProperty(string name, object? value) =>
        ScopedValue.Inspect(scope =>
            scope.AddOrUpdate(name, value, (_, _) => value));
}
