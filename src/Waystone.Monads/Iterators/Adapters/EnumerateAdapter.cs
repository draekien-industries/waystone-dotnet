namespace Waystone.Monads.Iterators.Adapters;

using Abstractions;
using Options;

/// <summary>
/// Represents an iterator that enumerates items from a source iterator
/// and associates each item with its respective index.
/// </summary>
/// <typeparam name="TItem">
/// The type of the items being enumerated. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class EnumerateAdapter<TItem> : Iterator<(int, TItem)>
    where TItem : notnull
{
    private readonly IIterator<TItem> _source;

    /// <summary>
    /// An iterator that enumerates over a source iterator, providing each
    /// item along with its index.
    /// </summary>
    public EnumerateAdapter(IIterator<TItem> source)
    {
        _source = source;
    }

    /// <inheritdoc />
    public override Option<(int, TItem)> Next() =>
        _source.Next().Map(item => (_source.Position, x: item));

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }

    /// <inheritdoc />
    public override void Reset()
    {
        _source.Reset();
        base.Reset();
    }
}
