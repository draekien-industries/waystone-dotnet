namespace Waystone.Monads.Iterators.Adapters;

using System;
using Abstractions;
using Options;

/// <summary>
/// Represents an adapter that scans a source iterator while maintaining
/// an internal state and producing outputs based on a transformation function.
/// </summary>
/// <typeparam name="TItem">The type of elements in the source iterator.</typeparam>
/// <typeparam name="TState">The type of the state used during the scan operation.</typeparam>
/// <typeparam name="TOut">
/// The type of the output elements produced by the scan
/// operation.
/// </typeparam>
public sealed class ScanAdapter<TItem, TState, TOut> : Iterator<TOut>
    where TItem : notnull where TState : notnull where TOut : notnull
{
    private readonly Func<TState, TItem, Option<TOut>> _scan;
    private readonly IIterator<TItem> _source;
    private readonly TState _state;

    /// <summary>
    /// An adapter that processes a source <see cref="IIterator{TItem}" />
    /// using a scan operation. Maintains an internal state throughout the iteration
    /// and produces transformations of the source elements.
    /// </summary>
    public ScanAdapter(
        IIterator<TItem> source,
        TState initialState,
        Func<TState, TItem, Option<TOut>> scan)
    {
        _source = source;
        _state = initialState;
        _scan = scan;
    }

    /// <inheritdoc />
    public override Option<TOut> Next()
    {
        return _source.Next().FlatMap(item => _scan(_state, item));
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }
}
