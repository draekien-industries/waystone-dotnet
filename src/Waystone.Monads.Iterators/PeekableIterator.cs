namespace Waystone.Monads.Iterators;

using Abstractions;
using Options;

/// <summary>
/// The <c>Peekable</c> class extends the behavior of an
/// <see cref="IIterator{TItem}" /> by allowing the user to "peek" at the next item
/// in the sequence without advancing the iterator.
/// </summary>
/// <typeparam name="TItem">
/// The type of items being iterated. Must be a
/// non-nullable type.
/// </typeparam>
/// <typeparam name="TIter">
/// The type of the underlying iterator implementation.
/// Must implement <see cref="IIterator{TItem}" />.
/// </typeparam>
public sealed class PeekableIterator<TItem, TIter> : Iterator<TItem>
    where TIter : IIterator<TItem>
    where TItem : notnull
{
    private readonly TIter _source;
    private bool _hasPeeked;
    private Option<TItem> _peeked;

    /// <summary>
    /// A specialized iterator that extends the functionality of an
    /// <see cref="IIterator{TItem}" /> by enabling the ability to look at the next
    /// item in the sequence without advancing the iterator.
    /// </summary>
    /// <typeparam name="TItem">
    /// Specifies the type of elements returned by the
    /// iterator. Must be non-nullable.
    /// </typeparam>
    /// <typeparam name="TIter">
    /// Defines the implementation of the underlying iterator.
    /// Must implement <see cref="IIterator{TItem}" />.
    /// </typeparam>
    public PeekableIterator(TIter source)
    {
        _source = source;
        _hasPeeked = false;
        _peeked = Option.None<TItem>();
    }

    /// <inheritdoc />
    public override Option<TItem> Next()
    {
        if (!_hasPeeked) return _source.Next();
        _hasPeeked = false;
        return _peeked;
    }

    /// <summary>
    /// Retrieves the next item in the sequence without advancing the
    /// iterator. If the item has previously been peeked, it returns the same value;
    /// otherwise, it fetches the next item from the underlying iterator and caches it
    /// for future reference.
    /// </summary>
    /// <returns>
    /// An <see cref="Option{TItem}" /> that represents the next item in the
    /// sequence. If no items remain in the sequence, it returns a value indicating no
    /// result.
    /// </returns>
    public Option<TItem> Peek()
    {
        if (_hasPeeked)
        {
            return _peeked;
        }

        _peeked = _source.Next();
        _hasPeeked = true;
        return _peeked;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }

    /// <inheritdoc />
    public override void Reset()
    {
        _source.Reset();
        _hasPeeked = false;
        _peeked = Option.None<TItem>();
        base.Reset();
    }
}
