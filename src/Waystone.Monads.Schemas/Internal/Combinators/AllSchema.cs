namespace Waystone.Monads.Schemas.Internal.Combinators;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AllSchema<TIn, TOut> : Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly Schema<TIn, TOut>[] _branches;

    internal AllSchema(Schema<TIn, TOut>[] branches)
    {
        _branches = Branches.Require(branches);
    }

    internal override Outcome<TOut> Evaluate(TIn input, ParseContext context)
    {
        Outcome<TOut> lead = _branches[0].Evaluate(input, context);
        List<Violation>? violations = Gather(null, lead);

        for (var index = 1; index < _branches.Length; index++)
        {
            violations = Gather(
                violations,
                _branches[index].Evaluate(input, context));
        }

        return Combine(lead, violations);
    }

    internal override async ValueTask<Outcome<TOut>> EvaluateAsync(
        TIn input,
        ParseContext context,
        CancellationToken cancellationToken)
    {
        Outcome<TOut> lead = await _branches[0]
                                  .EvaluateAsync(
                                       input,
                                       context,
                                       cancellationToken)
                                  .ConfigureAwait(false);

        List<Violation>? violations = Gather(null, lead);

        for (var index = 1; index < _branches.Length; index++)
        {
            Outcome<TOut> outcome = await _branches[index]
                                         .EvaluateAsync(
                                              input,
                                              context,
                                              cancellationToken)
                                         .ConfigureAwait(false);

            violations = Gather(violations, outcome);
        }

        return Combine(lead, violations);
    }

    private static List<Violation>? Gather(
        List<Violation>? violations,
        Outcome<TOut> outcome)
    {
        if (outcome.Violations.Count == 0) return violations;

        violations ??= new List<Violation>();
        violations.AddRange(outcome.Violations);

        return violations;
    }

    private static Outcome<TOut> Combine(
        Outcome<TOut> lead,
        List<Violation>? violations) =>
        violations is null ? lead : lead.WithViolations(violations);
}
