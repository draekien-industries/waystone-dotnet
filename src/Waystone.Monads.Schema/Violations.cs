namespace Waystone.Monads.Schemas;

using System.Collections.Generic;
using Waystone.Monads.Results.Errors;

internal static class Violations
{
    internal static IReadOnlyList<Violation> One(
        ParseContext context,
        ErrorCode code,
        string template,
        object? received = null) =>
        new[]
        {
            new Violation(
                context.Path,
                code,
                MessageTemplate.Render(
                    template,
                    context.Path,
                    code,
                    received,
                    context.IsSensitive)),
        };
}
