namespace Waystone.Monads.Schemas;

using System;
using System.Threading;
using System.Threading.Tasks;
using Waystone.Monads.Results.Errors;

internal sealed class AsyncCheckSchema<TIn, TOut> : Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    internal const string SynchronousParseMessage =
        "This schema contains an asynchronous rule, so Parse cannot run it. Call ParseAsync instead, or replace the rule with a synchronous Check.";

    private readonly ErrorCode _code;

    private readonly Schema<TIn, TOut> _inner;

    private readonly string _message;

    private readonly Func<TOut, CancellationToken, ValueTask<bool>> _predicate;

    /// <remarks>
    /// The inner schema is unguarded on purpose: the only caller is
    /// <c>Schema.CheckAsync</c>, which passes itself.
    /// </remarks>
    internal AsyncCheckSchema(
        Schema<TIn, TOut> inner,
        Func<TOut, CancellationToken, ValueTask<bool>> predicate,
        ErrorCode code,
        string message)
    {
        _inner = inner;

        _predicate = predicate
                  ?? throw new ArgumentNullException(nameof(predicate));

        _code = code ?? throw new ArgumentNullException(nameof(code));
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    internal override Outcome<TOut> Evaluate(TIn input, ParseContext context) =>
        throw new InvalidOperationException(SynchronousParseMessage);

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

        bool satisfied = await _predicate(outcome.Value, cancellationToken)
                            .ConfigureAwait(false);

        if (satisfied) return outcome;

        return Outcome<TOut>.Refined(
            outcome.Value,
            Violations.Add(
                outcome.Violations,
                context,
                _code,
                _message,
                outcome.Value));
    }
}
