namespace Waystone.Monads.Iterators;

using Abstractions;
using Options;

/// <summary>
/// Represents a chainable iterator that combines two separate iterators
/// into a single iterator. It attempts to retrieve the next item from the first
/// iterator, and if no items are left, it continues with the second iterator.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class ChainIterator<TItem> : Iterator<TItem>
    where TItem : notnull
{
    private readonly IIterator<TItem> _first;
    private readonly IIterator<TItem> _second;

    /// <summary>
    /// Represents a chainable iterator that attempts to retrieve the next
    /// item from the first iterator, and if no items are left, continues with the
    /// second iterator.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of the item to iterate over. Must be a
    /// non-nullable type.
    /// </typeparam>
    public ChainIterator(IIterator<TItem> first, IIterator<TItem> second)
    {
        _first = first;
        _second = second;
    }

    /// <inheritdoc />
    public override Option<TItem> Next() =>
        _first.Next().OrElse(() => _second.Next());

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _first.Dispose();
        _second.Dispose();
    }
}
