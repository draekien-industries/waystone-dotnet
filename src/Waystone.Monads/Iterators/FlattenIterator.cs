namespace Waystone.Monads.Iterators;

using System.Collections.Generic;
using System.Linq;

/// <summary></summary>
/// <typeparam name="TItem"></typeparam>
public sealed class FlattenIterator<TItem> : Iterator<TItem>
    where TItem : notnull
{
    /// <inheritdoc />
    public FlattenIterator(IEnumerable<IEnumerable<TItem>> source) : base(
        source.SelectMany(x => x))
    { }

    /// <inheritdoc />
    public FlattenIterator(Iterator<IEnumerable<TItem>> source) : base(
        source.Source.SelectMany(x => x))
    { }
}
