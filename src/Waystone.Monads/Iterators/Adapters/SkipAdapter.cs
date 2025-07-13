namespace Waystone.Monads.Iterators.Adapters;

using System;
using Abstractions;
using Options;

/// <summary>
/// Represents an iterator adapter that skips a specified number of
/// elements in the source iterator before yielding the rest of the items.
/// </summary>
/// <typeparam name="TItem">The type of the elements in the sequence.</typeparam>
public sealed class SkipAdapter<TItem> : Iterator<TItem> where TItem : notnull
{
    private readonly int _skip;
    private readonly IIterator<TItem> _source;
    private bool _skipped;

    /// <summary>Creates an instance of the <see cref="SkipAdapter{TItem}" /></summary>
    /// <param name="source">The source iterator</param>
    /// <param name="skip">The number of items to skip</param>
    /// <exception cref="ArgumentOutOfRangeException">Skip must be greater than zero</exception>
    public SkipAdapter(IIterator<TItem> source, int skip)
    {
        if (skip <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(skip),
                "Skip must be greater than zero.");
        }

        _source = source;
        _skip = skip;
        _skipped = false;
    }

    /// <inheritdoc />
    public override Option<TItem> Next()
    {
        if (_skipped) return _source.Next();

        Option<TItem> result = _source.Nth(_skip);
        _skipped = true;
        return result;
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
        _skipped = false;
        base.Reset();
    }
}
