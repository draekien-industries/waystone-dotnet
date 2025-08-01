namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using Options;

/// <summary>
/// Represents an iterator that applies a filter and a mapping function to
/// an input sequence and produces a new sequence of results.
/// </summary>
/// <typeparam name="TItem">The type of elements in the input sequence.</typeparam>
/// <typeparam name="TOut">The type of elements in the output sequence.</typeparam>
public sealed class FilterMapIterator<TItem, TOut> : Iterator<TOut>
    where TItem : notnull where TOut : notnull
{
    /// <inheritdoc />
    public FilterMapIterator(
        IEnumerable<TItem> source,
        Func<TItem, Option<TOut>> mapper) : this(
        source.IntoIter(),
        mapper)
    { }

    /// <inheritdoc />
    public FilterMapIterator(
        Iterator<TItem> source,
        Func<TItem, Option<TOut>> mapper) : base(
        source.Map(mapper).Filter(x => x.IsSome).Map(x => x.Unwrap()))
    { }
}
