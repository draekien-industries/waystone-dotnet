namespace Waystone.Monads.Schemas.Internal.Combinators;

internal abstract class ContextSchema<TIn, TOut>
    : DecoratorSchema<TIn, TOut, TOut>
    where TIn : notnull where TOut : notnull
{
    protected ContextSchema(Schema<TIn, TOut> inner) : base(inner)
    {
    }

    protected sealed override Outcome<TOut> Decorate(
        TIn input,
        ParseContext context,
        Outcome<TOut> outcome) =>
        outcome;

    protected abstract override ParseContext Adjust(ParseContext context);
}
