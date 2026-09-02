namespace Waystone.Monads.Schemas.Internal.Combinators;

using System;
using System.Threading;
using System.Threading.Tasks;

internal abstract class DecoratorSchema<TIn, TOut, TNext> : Schema<TIn, TNext>
    where TIn : notnull where TOut : notnull where TNext : notnull
{
    private readonly Schema<TIn, TOut> _inner;

    protected DecoratorSchema(Schema<TIn, TOut> inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    internal sealed override Outcome<TNext> Evaluate(
        TIn input,
        ParseContext context) =>
        Decorate(input, context, _inner.Evaluate(input, Adjust(context)));

    internal sealed override async ValueTask<Outcome<TNext>> EvaluateAsync(
        TIn input,
        ParseContext context,
        CancellationToken cancellationToken)
    {
        Outcome<TOut> outcome = await _inner
                                     .EvaluateAsync(
                                          input,
                                          Adjust(context),
                                          cancellationToken)
                                     .ConfigureAwait(false);

        return Decorate(input, context, outcome);
    }

    protected abstract Outcome<TNext> Decorate(
        TIn input,
        ParseContext context,
        Outcome<TOut> outcome);

    protected virtual ParseContext Adjust(ParseContext context) => context;
}
