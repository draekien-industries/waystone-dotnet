namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DictionarySchema<TKeyIn, TValueIn, TKeyOut, TValueOut>
    : Schema<IReadOnlyDictionary<TKeyIn, TValueIn>,
        IReadOnlyDictionary<TKeyOut, TValueOut>>
    where TKeyIn : notnull
    where TValueIn : notnull
    where TKeyOut : notnull
    where TValueOut : notnull
{
    private readonly Schema<TKeyIn, TKeyOut> _key;

    private readonly Schema<TValueIn, TValueOut> _value;

    internal DictionarySchema(
        Schema<TKeyIn, TKeyOut> key,
        Schema<TValueIn, TValueOut> value)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal override Outcome<IReadOnlyDictionary<TKeyOut, TValueOut>> Evaluate(
        IReadOnlyDictionary<TKeyIn, TValueIn> input,
        ParseContext context)
    {
        var pairs = new Pairs<TKeyOut, TValueOut>(input.Count, context);

        foreach (KeyValuePair<TKeyIn, TValueIn> entry in input)
        {
            if (pairs.IsFull) break;

            ParseContext at = At(context, entry.Key);

            pairs.Take(
                at,
                _key.Evaluate(entry.Key, at),
                entry.Value is null
                    ? Violations.Absent<TValueOut>(at)
                    : _value.Evaluate(entry.Value, at));
        }

        return pairs.ToOutcome();
    }

    internal override async
        ValueTask<Outcome<IReadOnlyDictionary<TKeyOut, TValueOut>>>
        EvaluateAsync(
            IReadOnlyDictionary<TKeyIn, TValueIn> input,
            ParseContext context,
            CancellationToken cancellationToken)
    {
        var pairs = new Pairs<TKeyOut, TValueOut>(input.Count, context);

        foreach (KeyValuePair<TKeyIn, TValueIn> entry in input)
        {
            if (pairs.IsFull) break;

            ParseContext at = At(context, entry.Key);

            Outcome<TKeyOut> key = await _key
                                        .EvaluateAsync(
                                             entry.Key,
                                             at,
                                             cancellationToken)
                                        .ConfigureAwait(false);

            Outcome<TValueOut> value = entry.Value is null
                ? Violations.Absent<TValueOut>(at)
                : await _value.EvaluateAsync(entry.Value, at, cancellationToken)
                              .ConfigureAwait(false);

            pairs.Take(at, key, value);
        }

        return pairs.ToOutcome();
    }

    private static ParseContext At(ParseContext context, TKeyIn key) =>
        context.AtKey(key.ToString() ?? string.Empty);
}
