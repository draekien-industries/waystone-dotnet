namespace Waystone.Monads.Schemas.Internal.Reporting;

using System.Collections.Generic;
using Waystone.Monads.Results.Errors;

internal static class Violations
{
    internal const string AbsentMessage = "Expected {Path} to be present.";

    internal static Outcome<T> Absent<T>(ParseContext context)
        where T : notnull =>
        Outcome<T>.Failed(
            One(context, ViolationCodeCatalog.Codes.Incomplete, AbsentMessage));

    internal static Violation Create(
        ParseContext context,
        ErrorCode code,
        string template,
        object? received = null,
        object? expected = null,
        string? predicate = null) =>
        new(
            context.Path,
            code,
            template,
            received,
            expected,
            predicate,
            context.IsSensitive);

    internal static IReadOnlyList<Violation> One(
        ParseContext context,
        ErrorCode code,
        string template,
        object? received = null,
        object? expected = null) =>
        new[] { Create(context, code, template, received, expected) };

    internal static IReadOnlyList<Violation> Add(
        IReadOnlyList<Violation> existing,
        ParseContext context,
        ErrorCode code,
        string template,
        object? received = null,
        object? expected = null,
        string? predicate = null) =>
        Append(
            existing,
            Create(context, code, template, received, expected, predicate));

    internal static IReadOnlyList<Violation> Concat(
        IReadOnlyList<Violation> first,
        IReadOnlyList<Violation> second)
    {
        if (second.Count == 0) return first;

        Violation[] violations = Grow(first, second.Count);

        for (var index = 0; index < second.Count; index++)
        {
            violations[first.Count + index] = second[index];
        }

        return violations;
    }

    private static IReadOnlyList<Violation> Append(
        IReadOnlyList<Violation> existing,
        Violation violation)
    {
        Violation[] violations = Grow(existing, 1);

        violations[existing.Count] = violation;

        return violations;
    }

    private static Violation[] Grow(
        IReadOnlyList<Violation> existing,
        int additional)
    {
        var violations = new Violation[existing.Count + additional];

        for (var index = 0; index < existing.Count; index++)
        {
            violations[index] = existing[index];
        }

        return violations;
    }
}
