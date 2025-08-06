namespace Waystone.Monads.Iterators.Adapters;

using System;
using System.Collections.Generic;
using System.Linq;
using Options;

/// <summary>
/// An <see cref="Iter{TOut}" /> that filters and maps elements from an
/// input sequence of type <typeparamref name="TIn" /> to an output sequence of
/// type <typeparamref name="TOut" />. It applies a filtering and mapping function
/// to each element of the input sequence to produce the corresponding element in
/// the output sequence. If the filtering function returns
/// <see cref="Option.None{TOut}" />, that element is excluded from the output
/// sequence.
/// </summary>
/// <typeparam name="TIn">
/// The type of elements in the input sequence. Must be a
/// non-nullable type.
/// </typeparam>
/// <typeparam name="TOut">
/// The type of elements in the output sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class FilterMap<TIn, TOut> : Iter<TOut>
    where TIn : notnull
    where TOut : notnull
{
    /// <inheritdoc />
    internal FilterMap(
        IEnumerable<TIn> elements,
        Func<TIn, Option<TOut>> filterAndMap) : base(
        elements.Select(filterAndMap))
    { }

    /// <inheritdoc />
    internal FilterMap(Iter<TIn> iter, Func<TIn, Option<TOut>> filterAndMap) :
        this(iter.Elements, filterAndMap)
    { }
}
