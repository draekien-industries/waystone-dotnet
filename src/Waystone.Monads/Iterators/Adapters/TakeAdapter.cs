namespace Waystone.Monads.Iterators.Adapters;

using System;
using Abstractions;
using Options;

/// <summary>
/// Represents an iterator adapter that limits the number of items to be
/// enumerated or processed from the source iterator.
/// </summary>
/// <typeparam name="TItem">
/// The type of the items being iterated, which must be
/// non-nullable.
/// </typeparam>
public sealed class TakeAdapter<TItem> : Iterator<TItem> where TItem : notnull
{
    private readonly IIterator<TItem> _source;
    private readonly int _take;
    private int _taken;

    /// <summary>
    /// Represents an adapter that limits the number of elements to be
    /// enumerated from a given source iterator.
    /// </summary>
    public TakeAdapter(IIterator<TItem> source, int take)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(take),
                "Take must be greater than zero.");
        }

        _source = source;
        _take = take;
        _taken = 0;
    }


    /// <inheritdoc />
    public override Option<TItem> Next()
    {
        if (_taken >= _take) return Option.None<TItem>();
        _taken++;
        return _source.Next();
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
        _taken = 0;
        base.Reset();
    }
}
