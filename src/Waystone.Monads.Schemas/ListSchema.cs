namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ListSchema<TIn, TOut>
    : Schema<IReadOnlyList<TIn>, IReadOnlyList<TOut>>
    where TIn : notnull where TOut : notnull
{
    private readonly Schema<TIn, TOut> _item;

    internal ListSchema(Schema<TIn, TOut> item)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
    }

    internal override Outcome<IReadOnlyList<TOut>> Evaluate(
        IReadOnlyList<TIn> input,
        ParseContext context)
    {
        var entries = new Entries<TOut>(input.Count);

        for (var index = 0; index < input.Count; index++)
        {
            ParseContext at = context.AtIndex(index);
            TIn item = input[index];

            entries.Take(
                index,
                item is null
                    ? Violations.Absent<TOut>(at)
                    : _item.Evaluate(item, at));
        }

        return entries.ToOutcome();
    }

    internal override async ValueTask<Outcome<IReadOnlyList<TOut>>>
        EvaluateAsync(
            IReadOnlyList<TIn> input,
            ParseContext context,
            CancellationToken cancellationToken)
    {
        var entries = new Entries<TOut>(input.Count);

        for (var index = 0; index < input.Count; index++)
        {
            ParseContext at = context.AtIndex(index);
            TIn item = input[index];

            Outcome<TOut> outcome = item is null
                ? Violations.Absent<TOut>(at)
                : await _item.EvaluateAsync(item, at, cancellationToken)
                             .ConfigureAwait(false);

            entries.Take(index, outcome);
        }

        return entries.ToOutcome();
    }
}
