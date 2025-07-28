namespace Waystone.Monads.Iterators;

using System;
using System.Collections.Generic;
using System.Linq;

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
public sealed class ClonedIterator<TItem> : Iterator<TItem>
    where TItem : class, ICloneable
{
    /// <inheritdoc />
    public ClonedIterator(IEnumerable<TItem> source) : base(
        source.Select(x => x.Clone()).Cast<TItem>())
    { }
}
