namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a mapping iterator that transforms elements from a source
/// sequence using a specified mapping function.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the source sequence. Must be a
/// non-nullable type.
/// </typeparam>
/// <typeparam name="TOut">
/// The type of elements in the resulting sequence after
/// applying the mapping function. Must be a non-nullable type.
/// </typeparam>
public sealed class Map<TItem, TOut> : Iterator<TOut>
    where TItem : notnull where TOut : notnull
{
    /// <inheritdoc />
    public Map(IEnumerable<TItem> source, Func<TItem, TOut> mapper) : base(
        source.Select(mapper))
    { }
}
