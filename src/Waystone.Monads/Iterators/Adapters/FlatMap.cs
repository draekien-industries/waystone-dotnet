namespace Waystone.Monads.Iterators.Adapters;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Creates an <see cref="Iter{T}" /> that works like a
/// <see cref="Map{TIn,TOut}" /> but flattens nested structures.
/// </summary>
/// <typeparam name="TIn">
/// The type of elements in the input sequence. Must be a
/// non-nullable type.
/// </typeparam>
/// <typeparam name="TOut">
/// The type of elements in the output sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class FlatMap<TIn, TOut> : Iter<TOut>
    where TIn : notnull
    where TOut : notnull
{
    /// <inheritdoc />
    internal FlatMap(
        IEnumerable<TIn> elements,
        Func<TIn, IEnumerable<TOut>> map)
        : base(elements.SelectMany(map))
    { }

    /// <inheritdoc />
    internal FlatMap(Iter<TIn> iter, Func<TIn, IEnumerable<TOut>> map) : this(
        iter.Elements,
        map)
    { }
}
