namespace Waystone.Monads.Iterators.Adapters;

using System;
using System.Collections.Generic;
using Abstractions;
using Extensions;
using Options;

/// <summary>
/// Represents an adapter that allows an iterator to flatten collections
/// produced by mapping each item into an <see cref="IEnumerable{TOut}" />.
/// Produces a new, flattened iterator of type <typeparamref name="TOut" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of the items in the source iterator. Must be a
/// non-nullable type.
/// </typeparam>
/// <typeparam name="TOut">
/// The type of the items produced after mapping and
/// flattening the source iterator. Must be a non-nullable type.
/// </typeparam>
public sealed class FlatMapAdapter<TItem, TOut> : Iterator<TOut>
    where TItem : notnull where TOut : notnull
{
    private readonly Func<TItem, IEnumerable<TOut>> _mapper;
    private readonly IIterator<TItem> _source;
    private Option<IIterator<TOut>> _current;

    /// <summary>
    /// Represents an adapter that flattens mapped results from an iterator.
    /// The adapter maps each item from its source iterator using a specified function
    /// and then flattens the resulting collections into a single, continuous sequence
    /// of items.
    /// </summary>
    public FlatMapAdapter(
        IIterator<TItem> source,
        Func<TItem, IEnumerable<TOut>> mapper)
    {
        _source = source;
        _mapper = mapper;
        _current = Option.None<IIterator<TOut>>();
    }

    /// <inheritdoc />
    public override Option<TOut> Next()
    {
        if (_current.IsNone)
        {
            _current = _source.Next().Map(_mapper).Map(x => x.IntoIter());
        }

        return _current.FlatMap(x => x.Next());
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        throw new NotImplementedException();
    }
}
