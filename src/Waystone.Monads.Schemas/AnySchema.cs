namespace Waystone.Monads.Schemas;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AnySchema<TIn, TOut> : Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    private const string ExhaustedMessage =
        "Expected {Path} to satisfy one of the permitted alternatives.";

    private readonly Schema<TIn, TOut>[] _branches;

    internal AnySchema(Schema<TIn, TOut>[] branches)
    {
        _branches = Branches.Require(branches);
    }

    internal override Outcome<TOut> Evaluate(TIn input, ParseContext context)
    {
        var rejected = new List<Violation>(_branches.Length);

        for (var index = 0; index < _branches.Length; index++)
        {
            Outcome<TOut> outcome =
                _branches[index].Evaluate(input, context.AtBranch(index));

            if (outcome.Violations.Count == 0) return outcome;

            rejected.AddRange(outcome.Violations);
        }

        return Exhausted(rejected, context, input);
    }

    internal override async ValueTask<Outcome<TOut>> EvaluateAsync(
        TIn input,
        ParseContext context,
        CancellationToken cancellationToken)
    {
        var rejected = new List<Violation>(_branches.Length);

        for (var index = 0; index < _branches.Length; index++)
        {
            Outcome<TOut> outcome = await _branches[index]
                                         .EvaluateAsync(
                                              input,
                                              context.AtBranch(index),
                                              cancellationToken)
                                         .ConfigureAwait(false);

            if (outcome.Violations.Count == 0) return outcome;

            rejected.AddRange(outcome.Violations);
        }

        return Exhausted(rejected, context, input);
    }

    private static Outcome<TOut> Exhausted(
        List<Violation> rejected,
        ParseContext context,
        TIn input)
    {
        var violations = new List<Violation>(rejected.Count + 1)
        {
            Violations.Create(
                context,
                ViolationCodeCatalog.Codes.Mismatched,
                ExhaustedMessage,
                input),
        };

        violations.AddRange(rejected);

        return Outcome<TOut>.Failed(violations);
    }
}
