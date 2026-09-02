namespace Waystone.Monads.Schemas.Internal.Combinators;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class NotSchema<TIn, TOut> : Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly Schema<TIn, TOut> _inner;

    private readonly string _message;

    private readonly Schema<TIn, TOut> _rejected;

    internal NotSchema(
        Schema<TIn, TOut> inner,
        Schema<TIn, TOut> rejected,
        string message)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        _rejected = rejected
                 ?? throw new ArgumentNullException(nameof(rejected));

        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    internal override Outcome<TOut> Evaluate(TIn input, ParseContext context)
    {
        Outcome<TOut> outcome = _inner.Evaluate(input, context);

        return outcome.HasValue
            ? Combine(outcome, _rejected.Evaluate(input, context), context)
            : outcome;
    }

    internal override async ValueTask<Outcome<TOut>> EvaluateAsync(
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

        if (!outcome.HasValue) return outcome;

        Outcome<TOut> rejected = await _rejected
                                      .EvaluateAsync(
                                           input,
                                           context,
                                           cancellationToken)
                                      .ConfigureAwait(false);

        return Combine(outcome, rejected, context);
    }

    private Outcome<TOut> Combine(
        Outcome<TOut> outcome,
        Outcome<TOut> rejected,
        ParseContext context) =>
        rejected.Violations.Count > 0
            ? outcome
            : Outcome<TOut>.Refined(
                  outcome.Value,
                  Violations.Add(
                      outcome.Violations,
                      context,
                      ViolationCodeCatalog.Codes.NotAllowed,
                      _message,
                      outcome.Value));
}
