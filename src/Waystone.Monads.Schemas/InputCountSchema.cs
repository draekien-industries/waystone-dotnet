namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Rejects an over-long collection before its entries are parsed, rather than
/// after.
/// </summary>
/// <remarks>
/// The one size rule that is not an ordinary decorator, and it has to be. A
/// decorator runs on what the inner schema produced, so bounding a list that way
/// means parsing every entry of a list already known to be too long — which is an
/// unbounded amount of work on behalf of whoever sent it. This sits in front
/// instead, and is reachable only where the input is itself the collection being
/// counted.
/// </remarks>
internal sealed class InputCountSchema<TIn, TOut> : Schema<TIn, TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly Func<TIn, int> _count;

    private readonly Schema<TIn, TOut> _inner;

    private readonly int _max;

    internal InputCountSchema(
        Schema<TIn, TOut> inner,
        Func<TIn, int> count,
        int max)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _count = count;
        _max = max;
    }

    internal override Outcome<TOut> Evaluate(TIn input, ParseContext context) =>
        TooMany(input)
            ? Refuse(context, input)
            : _inner.Evaluate(input, context);

    internal override ValueTask<Outcome<TOut>> EvaluateAsync(
        TIn input,
        ParseContext context,
        CancellationToken cancellationToken) =>
        TooMany(input)
            ? new ValueTask<Outcome<TOut>>(Refuse(context, input))
            : _inner.EvaluateAsync(input, context, cancellationToken);

    private bool TooMany(TIn input) => _count(input) > _max;

    private Outcome<TOut> Refuse(ParseContext context, TIn input) =>
        Outcome<TOut>.Failed(
            Violations.One(
                context,
                ViolationCodeCatalog.Codes.OutOfRange,
                CollectionSchemaExtensions.TooMany,
                _count(input),
                _max));
}
