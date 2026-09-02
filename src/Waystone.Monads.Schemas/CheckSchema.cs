namespace Waystone.Monads.Schemas;

using System;
using Waystone.Monads.Results.Errors;

internal sealed class CheckSchema<TIn, TOut> : DecoratorSchema<TIn, TOut, TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly ErrorCode _code;

    private readonly object? _expected;

    private readonly string _message;

    private readonly Func<TOut, bool> _predicate;

    internal CheckSchema(
        Schema<TIn, TOut> inner,
        Func<TOut, bool> predicate,
        ErrorCode code,
        string message,
        object? expected = null) : base(inner)
    {
        _predicate = predicate
                  ?? throw new ArgumentNullException(nameof(predicate));

        _code = code ?? throw new ArgumentNullException(nameof(code));
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _expected = expected;
    }

    protected override Outcome<TOut> Decorate(
        TIn input,
        ParseContext context,
        Outcome<TOut> outcome)
    {
        if (!outcome.HasValue) return outcome;

        if (_predicate(outcome.Value)) return outcome;

        return Outcome<TOut>.Refined(
            outcome.Value,
            Violations.Add(
                outcome.Violations,
                context,
                _code,
                _message,
                outcome.Value,
                _expected));
    }
}
