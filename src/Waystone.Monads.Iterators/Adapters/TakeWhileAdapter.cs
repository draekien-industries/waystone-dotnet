namespace Waystone.Monads.Iterators.Adapters;

using Abstractions;
using Options;

/// <summary>
/// Represents an iterator that yields elements from a source iterator
/// until a specified predicate fails for the first time. Subsequent calls to the
/// iterator will not yield any elements once the predicate has been violated.
/// </summary>
/// <typeparam name="TItem">
/// The type of the elements being iterated over. Must be a
/// non-nullable type.
/// </typeparam>
public sealed class TakeWhileAdapter<TItem> : Iterator<TItem>
    where TItem : notnull
{
    private readonly Func<TItem, bool> _predicate;
    private readonly IIterator<TItem> _source;
    private bool _predicateFailed;

    /// <summary>
    /// Provides an iterator implementation that continues to yield elements
    /// from the underlying source iterator as long as the specified predicate
    /// evaluates to true. Once the predicate evaluates to false for the first time,
    /// the iteration stops, and no further elements will be produced.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of the elements being iterated over. Must be a
    /// non-nullable type.
    /// </typeparam>
    public TakeWhileAdapter(
        IIterator<TItem> source,
        Func<TItem, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
        _predicateFailed = false;
    }

    /// <inheritdoc />
    public override Option<TItem> Next()
    {
        if (_predicateFailed) return Option.None<TItem>();

        Option<TItem> next = _source.Next().Filter(_predicate);

        if (next.IsSome) return next;

        _predicateFailed = true;
        return next;
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
        _predicateFailed = false;
        base.Reset();
    }
}
