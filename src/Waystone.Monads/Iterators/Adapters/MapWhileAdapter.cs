namespace Waystone.Monads.Iterators.Adapters;

using System;
using Abstractions;
using Options;

/// <summary>
/// Represents an iterator that maps elements of type
/// <typeparamref name="TItem" /> to elements of type <typeparamref name="TOut" />,
/// stopping iteration when the mapping function returns none.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to be iterated over. Must be
/// non-nullable.
/// </typeparam>
/// <typeparam name="TOut">
/// The output type of the mapping function. Must be
/// non-nullable.
/// </typeparam>
public sealed class MapWhileAdapter<TItem, TOut> : Iterator<TOut>
    where TItem : notnull where TOut : notnull
{
    private readonly Func<TItem, Option<TOut>> _map;
    private readonly IIterator<TItem> _source;
    private bool _mapFailed;

    /// <summary>
    /// A sealed class that represents an iterator which transforms elements
    /// of type <typeparamref name="TItem" /> to elements of type
    /// <typeparamref name="TOut" />, halting iteration when the provided mapping
    /// function produces a `None` value.
    /// </summary>
    public MapWhileAdapter(
        IIterator<TItem> source,
        Func<TItem, Option<TOut>> map)
    {
        _source = source;
        _map = map;
        _mapFailed = false;
    }

    /// <inheritdoc />
    public override Option<TOut> Next()
    {
        if (_mapFailed) return Option.None<TOut>();

        Option<TOut> next = _source.Next().FlatMap(_map);

        if (next.IsNone) _mapFailed = true;

        return next;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }

    /// <inheritdoc />
    public override void Reset()
    {
        _source.Reset();
        _mapFailed = false;
        base.Reset();
    }
}
