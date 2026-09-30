namespace Waystone.Monads.Schemas.Internal.Combinators;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class PipeSchema<TIn, TOut, TNext> : Schema<TIn, TNext>
    where TIn : notnull where TOut : notnull where TNext : notnull
{
    private readonly Schema<TIn, TOut> _inner;

    private readonly Schema<TOut, TNext> _next;

    internal PipeSchema(Schema<TIn, TOut> inner, Schema<TOut, TNext> next)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    internal override Outcome<TNext> Evaluate(TIn input, ParseContext context)
    {
        Outcome<TOut> outcome = _inner.Evaluate(input, context);

        return outcome.HasValue
            ? Combine(outcome, _next.Evaluate(outcome.Value, context))
            : Outcome<TNext>.Failed(outcome.Violations);
    }

    internal override async ValueTask<Outcome<TNext>> EvaluateAsync(
        TIn input,
        ParseContext context,
        CancellationToken cancellationToken)
    {
        Outcome<TOut> outcome = await _inner
                                     .EvaluateAsync(
                                          input,
                                          context,
                                          cancellationToken)
                                     .ConfigureAwait(false);

        if (!outcome.HasValue) return Outcome<TNext>.Failed(outcome.Violations);

        Outcome<TNext> next = await _next
                                   .EvaluateAsync(
                                        outcome.Value,
                                        context,
                                        cancellationToken)
                                   .ConfigureAwait(false);

        return Combine(outcome, next);
    }

    private static Outcome<TNext> Combine(
        Outcome<TOut> outcome,
        Outcome<TNext> next) =>
        outcome.Violations.Count == 0
            ? next
            : next.WithViolations(
                Violations.Concat(outcome.Violations, next.Violations));
}
