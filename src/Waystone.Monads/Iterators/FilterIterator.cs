namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a filtering iterator that yields only elements that satisfy
/// a specified predicate.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the iterator. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class FilterIterator<TItem> : Iterator<TItem>
    where TItem : notnull
{
    /// <inheritdoc />
    public FilterIterator(
        IEnumerable<TItem> source,
        Func<TItem, bool> predicate) : base(source.Where(predicate))
    { }

    /// <inheritdoc />
    public FilterIterator(Iterator<TItem> source, Func<TItem, bool> predicate) :
        this(source.Source, predicate)
    { }
}
