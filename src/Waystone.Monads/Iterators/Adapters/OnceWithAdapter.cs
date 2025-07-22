namespace Waystone.Monads.Iterators.Adapters;

using System;
using Abstractions;
using Options;

/// <summary>
/// A sealed iterator adapter that transforms elements from a source
/// iterator using a mapping function and yields the transformed element exactly
/// once. Subsequent calls to retrieve next elements will return an empty option.
/// </summary>
/// <typeparam name="TItem">The type of the items in the source iterator.</typeparam>
/// <typeparam name="TOut">
/// The type of the items in the output iterator after
/// transformation.
/// </typeparam>
public sealed class OnceWithAdapter<TItem, TOut> : Iterator<TOut>
    where TItem : notnull where TOut : notnull
{
    private readonly Func<TItem, TOut> _mapper;
    private readonly IIterator<TItem> _source;
    private bool _hasBeenCalled;

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="OnceWithAdapter{TItem, TOut}" />
    /// </summary>
    /// <param name="source">The source iterator</param>
    /// <param name="mapper">The mapping function</param>
    public OnceWithAdapter(IIterator<TItem> source, Func<TItem, TOut> mapper)
    {
        _source = source;
        _mapper = mapper;
        _hasBeenCalled = false;
    }

    /// <inheritdoc />
    public override Option<TOut> Next()
    {
        if (_hasBeenCalled) return Option.None<TOut>();
        _hasBeenCalled = true;
        return _source.Next().Map(_mapper);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }
}
