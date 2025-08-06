namespace Waystone.Monads.Iterators.Adapters;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An <see cref="Iter{TOut}" /> that maps elements from an input sequence
/// of type <typeparamref name="TIn" /> to an output sequence of type
/// <typeparamref name="TOut" />. It applies a mapping function to each element of
/// the input sequence to produce the corresponding element in the output sequence.
/// </summary>
/// <typeparam name="TIn">
/// The type of elements in the input sequence. Must be a
/// non-nullable type.
/// </typeparam>
/// <typeparam name="TOut">
/// The type of elements in the output sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class Map<TIn, TOut> : Iter<TOut>
    where TIn : notnull where TOut : notnull
{
    /// <inheritdoc />
    internal Map(IEnumerable<TIn> elements, Func<TIn, TOut> map) : base(
        elements.Select(map))
    { }

    /// <inheritdoc />
    internal Map(Iter<TIn> iter, Func<TIn, TOut> map) : this(iter.Elements, map)
    { }
}
