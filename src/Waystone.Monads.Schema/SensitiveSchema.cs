namespace Waystone.Monads.Schemas;

using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SensitiveSchema<TIn, TOut> : SchemaNode<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly Schema<TIn, TOut> _inner;

    internal SensitiveSchema(Schema<TIn, TOut> inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    internal override Outcome<TOut> Evaluate(TIn input, ParseContext context) =>
        _inner.Evaluate(input, context.AsSensitive());

    internal override ValueTask<Outcome<TOut>> EvaluateAsync(
        TIn input,
        ParseContext context,
        CancellationToken cancellationToken) =>
        _inner.EvaluateAsync(
            input,
            context.AsSensitive(),
            cancellationToken);
}
