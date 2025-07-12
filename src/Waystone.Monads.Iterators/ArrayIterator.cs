namespace Waystone.Monads.Iterators;

using Abstractions;
using Options;

/// <summary>
/// Represents an iterator capable of iterating over an array of type
/// <typeparamref name="TItem" />. Provides support for forward iteration, backward
/// iteration, and determining the size of the collection.
/// </summary>
/// <typeparam name="TItem">The type of the items stored in the array.</typeparam>
public sealed class ArrayIterator<TItem>
    : Iterator<TItem>, IExactSizeIterator<TItem>, IDoubleEndedIterator<TItem>
    where TItem : notnull
{
    private readonly IEnumerator<TItem> _enumerator;
    private readonly TItem[] _source;

    /// <summary>
    /// Represents an iterator for an array of type
    /// <typeparamref name="TItem" />. Provides functionality for retrieving the
    /// current item, iterating forwards and backwards, resetting the iterator, and
    /// determining the length of the collection.
    /// </summary>
    /// <typeparam name="TItem">The type of elements in the array.</typeparam>
    public ArrayIterator(TItem[] source)
    {
        _source = source;
        _enumerator = source.AsEnumerable().GetEnumerator();

        PositionFromEnd = source.Length;
    }

    /// <summary>
    /// Gets the current element in the array when iterating backward. If the
    /// current position from the end is out of bounds, returns an empty
    /// <see cref="Option{T}" />.
    /// </summary>
    public Option<TItem> CurrentBack =>
        PositionFromEnd > Position && PositionFromEnd < _source.Length
            ? Option.Some(_source[PositionFromEnd])
            : Option.None<TItem>();

    /// <inheritdoc />
    public int PositionFromEnd { get; set; }

    /// <inheritdoc />
    public Option<TItem> NextBack()
    {
        if (PositionFromEnd <= Position) return Option.None<TItem>();
        PositionFromEnd--;
        return CurrentBack;
    }

    /// <inheritdoc />
    public override Option<TItem> Current =>
        Position >= 0 && Position < PositionFromEnd
            ? Option.Some(_source[Position])
            : Option.None<TItem>();

    /// <inheritdoc />
    public override Option<TItem> Next()
    {
        if (Position >= _source.Length || Position >= PositionFromEnd)
        {
            return Option.None<TItem>();
        }

        Position++;
        return Current;
    }

    /// <inheritdoc />
    public override void Reset()
    {
        _enumerator.Reset();
        PositionFromEnd = _source.Length;
        base.Reset();
    }

    /// <inheritdoc />
    public int Length => _source.Length;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _enumerator.Dispose();
    }
}
