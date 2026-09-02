namespace Waystone.Monads.Schemas.Internal.Combinators;

using System;

internal sealed class MapSchema<TIn, TOut, TNext>
    : DecoratorSchema<TIn, TOut, TNext>
    where TIn : notnull where TOut : notnull where TNext : notnull
{
    private readonly Func<TOut, TNext> _convert;

    internal MapSchema(Schema<TIn, TOut> inner, Func<TOut, TNext> convert)
        : base(inner)
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

        TNext next = _convert(outcome.Value)
                  ?? throw new InvalidOperationException(
                         "A conversion passed to Transform returned null. Use the overload returning a Result and return an Err instead, so the parse reports a violation rather than throwing.");

        return outcome.WithValue(next);
    }
}
