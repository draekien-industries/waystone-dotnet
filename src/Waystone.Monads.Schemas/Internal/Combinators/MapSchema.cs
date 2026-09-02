namespace Waystone.Monads.Schemas.Internal.Combinators;

using System;

internal sealed class MapSchema<TIn, TOut, TNext>
    : DecoratorSchema<TIn, TOut, TNext>
    where TIn : notnull where TOut : notnull where TNext : notnull
{
    internal const string NullMessage =
        "Expected {Path} to convert to a value, but the conversion produced none.";

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

        TNext next = _convert(outcome.Value);

        return next is null
            ? Outcome<TNext>.Failed(
                Violations.Add(
                    outcome.Violations,
                    context,
                    ViolationCodeCatalog.Codes.Malformed,
                    NullMessage,
                    outcome.Value))
            : outcome.WithValue(next);
    }
}
