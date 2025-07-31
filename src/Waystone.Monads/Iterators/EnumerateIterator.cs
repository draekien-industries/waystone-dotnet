namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using System.Linq;
using Options;

/// <summary>
/// Represents an iterator that enumerates over a sequence of items of
/// type <typeparamref name="TItem" /> and pairs each item with its corresponding
/// index in the sequence.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the enumerator. Must be a
/// non-nullable type.
/// </typeparam>
/// <remarks>
/// This iterator pairs each item in the source sequence with its
/// zero-based index and provides sequential access to these pairs wrapped in an
/// <see cref="Option{T}" />.
/// </remarks>
public sealed class EnumerateIterator<TItem>
    : Iterator<(int Index, TItem Value)> where TItem : notnull
{
    /// <inheritdoc />
    public EnumerateIterator(IEnumerable<TItem> source) :
        base(source.Select((x, i) => (i, x)))
    { }

    /// <inheritdoc />
    public EnumerateIterator(Iterator<TItem> source) : this(source.Source)
    { }
}
