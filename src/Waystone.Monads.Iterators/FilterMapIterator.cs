namespace Waystone.Monads.Iterators;

using Abstractions;
using Options;

/// <summary>
/// Represents an iterator that applies a filter and map transformation to
/// items of type <typeparamref name="TItem" /> from the source iterator, producing
/// transformed items of type <typeparamref name="TOut" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of items in the source iterator. Must be a
/// non-nullable type.
/// </typeparam>
/// <typeparam name="TOut">
/// The type of items produced by the filter and map
/// transformation. Must be a non-nullable type.
/// </typeparam>
public sealed class FilterMapIterator<TItem, TOut> : Iterator<TOut>
    where TItem : notnull where TOut : notnull
{
    private readonly Func<TItem, Option<TOut>> _filterMap;
    private readonly IIterator<TItem> _source;

    /// <summary>
    /// Represents an iterator that processes elements from a source iterator.
    /// Each element is transformed using a filter-map function, which combines
    /// filtering and mapping into a single operation. The resulting collection
    /// contains elements of the transformed type <typeparamref name="TOut" /> after
    /// applying the operation.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of elements in the source iterator. This must
    /// be a non-nullable type.
    /// </typeparam>
    /// <typeparam name="TOut">
    /// The type of elements resulting after the filter and map
    /// operation. This must be a non-nullable type.
    /// </typeparam>
    public FilterMapIterator(
        IIterator<TItem> source,
        Func<TItem, Option<TOut>> filterMap)
    {
        _source = source;
        _filterMap = filterMap;
    }

    /// <inheritdoc />
    public override Option<TOut> Next() => _source.Next().FlatMap(_filterMap);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }
}
