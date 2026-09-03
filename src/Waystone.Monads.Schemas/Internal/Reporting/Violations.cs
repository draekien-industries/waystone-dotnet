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

    private static IReadOnlyList<Violation> Append(
        IReadOnlyList<Violation> existing,
        Violation violation)
    {
        var violations = new Violation[existing.Count + 1];

        for (var index = 0; index < existing.Count; index++)
        {
            violations[index] = existing[index];
        }

        violations[existing.Count] = violation;

        return violations;
    }
}
