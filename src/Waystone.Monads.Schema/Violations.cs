namespace Waystone.Monads.Schemas;

using System.Collections.Generic;
using Waystone.Monads.Results.Errors;

internal static class Violations
{
    internal static Violation Create(
        ParseContext context,
        ErrorCode code,
        string template,
        object? received = null) =>
        new(
            context.Path,
            code,
            MessageTemplate.Render(
                template,
                context.Path,
                code,
                received,
                context.IsSensitive));

    internal static IReadOnlyList<Violation> One(
        ParseContext context,
        ErrorCode code,
        string template,
        object? received = null) =>
        new[] { Create(context, code, template, received) };

    internal static IReadOnlyList<Violation> Add(
        IReadOnlyList<Violation> existing,
        ParseContext context,
        ErrorCode code,
        string template,
        object? received = null)
    {
        var violations = new Violation[existing.Count + 1];

        for (var index = 0; index < existing.Count; index++)
        {
            violations[index] = existing[index];
        }

        violations[existing.Count] = Create(context, code, template, received);

        return violations;
    }
}
