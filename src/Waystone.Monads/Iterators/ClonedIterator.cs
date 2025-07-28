namespace Waystone.Monads.Iterators;

using System.Collections.Generic;

/// <summary>
/// Represents an iterator that clones the source collection to ensure
/// that the sequence remains the same regardless of changes to the original
/// source. Inherits from <see cref="Iterator{TItem}" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the iterator. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class ClonedIterator<TItem> : Iterator<TItem>
    where TItem : notnull
{
    /// <inheritdoc />
    public ClonedIterator(IEnumerable<TItem> source) : base([..source])
    { }
}
