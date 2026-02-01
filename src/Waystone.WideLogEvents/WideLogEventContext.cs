namespace Waystone.WideLogEvents;

using System.Collections.Generic;
using System.Threading;
using Monads.Options;

public sealed class WideLogEventContext
{
    internal static readonly
        AsyncLocal<Option<WideLogEventProperties>> ScopedProperties =
            new();

    public static WideLogEventScope BeginScope() => new();

    public static IReadOnlyDictionary<string, object?> GetScopedProperties() =>
        ScopedProperties.Value.Match(some => some, () => []);
}
