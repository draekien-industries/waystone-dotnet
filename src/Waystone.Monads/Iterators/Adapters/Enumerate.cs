namespace Waystone.Monads.Iterators.Adapters;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An <see cref="Iter{T}" /> that enumerates the elements of a sequence
/// along with their indices.
/// </summary>
/// <typeparam name="T">
/// The type of elements in the sequence. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class Enumerate<T> : Iter<(int Index, T Item)>
    where T : notnull
{
    /// <inheritdoc />
    public Enumerate(IEnumerable<T> elements) : base(
        elements.Select((x, i) => (i, x)))
    { }

    /// <inheritdoc />
    internal Enumerate(Iter<T> iter) : this(iter.Elements)
    { }
}
