namespace Waystone.Monads.Schemas;

using System;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

internal sealed class TransformSchema<TIn, TOut, TNext>
    : DecoratorSchema<TIn, TOut, TNext>
    where TIn : notnull where TOut : notnull where TNext : notnull
{
    private readonly Func<TOut, Result<TNext, Error>> _convert;

    internal TransformSchema(
        Schema<TIn, TOut> inner,
        Func<TOut, Result<TNext, Error>> convert) : base(inner)
    {
        _convert = convert ?? throw new ArgumentNullException(nameof(convert));
    }

    protected override Outcome<TNext> Decorate(
        TIn input,
        ParseContext context,
        Outcome<TOut> outcome)
    {
        if (!outcome.HasValue)
        {
            return Outcome<TNext>.Failed(outcome.Violations);
        }

        return _convert(outcome.Value)
           .Match(
                (outcome, context),
                static (next, state) => Succeed(next, state.outcome),
                static (error, state) => Fail(error, state.outcome, state.context));
    }

    private static Outcome<TNext> Succeed(TNext next, Outcome<TOut> outcome) =>
        outcome.WithValue(next);

    private static Outcome<TNext> Fail(
        Error error,
        Outcome<TOut> outcome,
        ParseContext context) =>
        Outcome<TNext>.Failed(
            Violations.Add(
                outcome.Violations,
                context,
                error.Code,
                error.Message,
                outcome.Value));
}
