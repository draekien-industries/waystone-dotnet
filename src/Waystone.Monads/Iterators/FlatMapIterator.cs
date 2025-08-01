namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using Extensions;

/// <summary>
/// An <see cref="Iterator{TItem}" /> that works like
/// <see cref="MapIterator{TItem,TOut}" />, but flattens nested structures like
/// <see cref="FlattenIterator{TItem}" />.
/// </summary>
/// <typeparam name="TItem">The type of the value in the source iterator.</typeparam>
/// <typeparam name="TOut">The type of the value after it has been mapped.</typeparam>
public sealed class FlatMapIterator<TItem, TOut> : Iterator<TOut>
    where TItem : notnull where TOut : notnull
{
    /// <inheritdoc />
    public FlatMapIterator(
        IEnumerable<TItem> source,
        Func<TItem, IEnumerable<TOut>> mapper) : base(
        source.IntoIter().Map(mapper).Flatten())
    { }

    /// <inheritdoc />
    public FlatMapIterator(
        Iterator<TItem> source,
        Func<TItem, IEnumerable<TOut>> mapper) : base(
        source.Map(mapper).Flatten())
    { }
}
