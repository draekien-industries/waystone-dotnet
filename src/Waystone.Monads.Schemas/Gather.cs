namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

internal static class Gather
{
    internal static Outcome<T> ToOutcome<T>(
        bool complete,
        T value,
        List<Violation> violations) where T : notnull
    {
        if (!complete) return Outcome<T>.Failed(violations);

        return violations.Count == 0
            ? Outcome<T>.Passed(value)
            : Outcome<T>.Refined(value, violations);
    }
}
