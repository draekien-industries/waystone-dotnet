namespace Waystone.Monads.Iterators.Adapters;

using System.Collections.Generic;
using System.Linq;

/// <summary>An <see cref="Iter{T}" /> that chains two sequences together.</summary>
/// <typeparam name="T">
/// The type of elements in the sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class Chain<T> : Iter<T> where T : notnull
{
    /// <inheritdoc />
    internal Chain(IEnumerable<T> items, IEnumerable<T> other) : base(
        items.Concat(other))
    { }

    /// <inheritdoc />
    internal Chain(Iter<T> iter, Iter<T> other) : this(
        iter.Elements,
        other.Elements)
    { }
}
