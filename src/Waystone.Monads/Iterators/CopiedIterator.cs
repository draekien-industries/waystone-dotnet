namespace Waystone.Monads.Iterators;

/// <summary>
/// Represents an iterator that clones the source collection to ensure
/// that the sequence remains the same regardless of changes to the original
/// source. Inherits from <see cref="Iterator{TItem}" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the iterator. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class CopiedIterator<TItem> : Iterator<TItem>
    where TItem : struct
{
    /// <inheritdoc />
    public CopiedIterator(Iterator<TItem> source) : base([..source.Source])
    { }
}
