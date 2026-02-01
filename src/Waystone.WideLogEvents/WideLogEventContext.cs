namespace Waystone.WideLogEvents;

using System.Collections.Generic;
using System.Threading;
using Monads.Options;

public sealed class WideLogEventContext
{
    internal static readonly
        AsyncLocal<Option<WideLogEventProperties>> ScopedProperties =
            new();

    public static WideLogEventScope CurrentScope =>
        WideLogEventScope.Resume(ScopedProperties.Value);

    public static WideLogEventScope BeginScope() => new();

    public static IReadOnlyDictionary<string, object?> GetProperties()
    {
        return ScopedProperties.Value.Match(some => some, () => []);
    }
}
