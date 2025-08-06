namespace Waystone.Monads.Iterators.Adapters;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An <see cref="Iter{T}" /> that filters elements based on a predicate.
/// It only includes elements for which the predicate returns true.
/// </summary>
/// <typeparam name="T">
/// The type of elements in the sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class Filter<T> : Iter<T> where T : notnull
{
    /// <inheritdoc />
    internal Filter(IEnumerable<T> elements, Func<T, bool> predicate) : base(
        elements.Where(predicate))
    { }

    /// <inheritdoc />
    internal Filter(Iter<T> iter, Func<T, bool> predicate) : this(
        iter.Elements,
        predicate)
    { }
}
