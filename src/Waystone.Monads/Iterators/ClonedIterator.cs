namespace Waystone.Monads.Iterators;

using System;
using Abstractions;

/// <summary>
/// An implementation of <see cref="Iterator{TItem}" /> that produces a
/// sequence of cloned elements from the source sequence. Each element in the
/// sequence is cloned using the <see cref="ICloneable.Clone" /> method to ensure
/// that modifications to individual elements do not affect the original source
/// sequence.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the sequence. Must be a
/// reference type and implement <see cref="ICloneable" />.
/// </typeparam>
public sealed class ClonedIterator<TItem>
    : Iterator<TItem>, ICloneable<ClonedIterator<TItem>>
    where TItem : class, ICloneable
{
    /// <inheritdoc />
    public ClonedIterator(Iterator<TItem> source) : base(
        source.Map(x => (TItem)x.Clone()))
    { }

    /// <inheritdoc />
    object ICloneable.Clone() => Clone();

    /// <inheritdoc />
    public void CloneFrom(ClonedIterator<TItem> other)
    {
        ClonedIterator<TItem> clone = other.Clone();

        Source = clone.Source;
        Current = clone.Current;
        NextCounter = clone.NextCounter;
        Disposed = clone.Disposed;
    }

    /// <inheritdoc />
    public ClonedIterator<TItem> Clone() => new(this);
}
