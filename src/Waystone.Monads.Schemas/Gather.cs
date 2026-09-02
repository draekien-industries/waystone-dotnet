namespace Waystone.Monads.Schemas;

using System.Collections.Generic;
using Waystone.Monads.Results;

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

    /// <summary>
    /// The single place violations become a failed result, so what a caller receives
    /// does not depend on which of the two paths into a result it took.
    /// </summary>
    /// <remarks>
    /// Only the failing half is shared. The passing half differs between the callers
    /// — one has a value in hand and the other builds one on demand — and folding
    /// them together would put a delegate on a path that has no need of one.
    /// </remarks>
    internal static Result<T, SchemaViolation> ToFailure<T>(
        IReadOnlyList<Violation> violations) where T : notnull =>
        Result.Err<T, SchemaViolation>(
            new SchemaViolation(new ViolationCollection(violations)));
}
