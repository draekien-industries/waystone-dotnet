namespace Waystone.Monads.Iterators.Abstractions;

using System;
using System.Collections;
using System.Collections.Generic;
using Adapters;
using Options;

/// <summary>
/// Represents an abstract implementation of a custom iterator for
/// iterating over a collection of items of type <typeparamref name="TItem" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of the item to iterate over. Must be a
/// non-nullable type.
/// </typeparam>
public abstract class Iterator<TItem> : IIterator<TItem> where TItem : notnull
{
    /// <inheritdoc />
    public virtual Option<TItem> Current { get; } =
        Option.None<TItem>();

    /// <inheritdoc />
    public bool MoveNext() => Next().IsSome;

    /// <inheritdoc />
    public virtual void Reset()
    {
        Position = -1;
    }

    /// <inheritdoc />
    object IEnumerator.Current => Current;

    /// <inheritdoc />
    public int Position { get; protected set; } = -1;

    /// <inheritdoc />
    public abstract Option<TItem> Next();

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual (int LowerBound, Option<int> UpperBound) SizeHint() =>
        (0, Option.None<int>());

    /// <inheritdoc />
    public virtual int Count()
    {
        var count = 0;
        while (Next().IsSome) count++;
        return count;
    }

    /// <inheritdoc />
    public virtual Option<TItem> Last()
    {
        Option<TItem> last = Option.None<TItem>();
        while (Next().IsSome) last = Next();
        return last;
    }

    /// <inheritdoc />
    public virtual Option<TItem> Nth(int n)
    {
        var count = 0;
        Option<TItem> nth = Next();

        while (nth.IsSome)
        {
            if (count == n) return nth;
            nth = Next();
            count++;
        }

        return nth;
    }

    /// <inheritdoc />
    public virtual StepByAdapter<TItem> StepBy(int step) =>
        new(this, step);

    /// <inheritdoc />
    public virtual ChainAdapter<TItem> Chain(IIterator<TItem> other) =>
        new(this, other);

    /// <inheritdoc />
    public virtual ZipAdapter<TItem, TOther> Zip<TOther>(
        IIterator<TOther> other) where TOther : notnull
        => new(this, other);

    /// <inheritdoc />
    public virtual MapAdapter<TItem, TOut> Map<TOut>(Func<TItem, TOut> map)
        where TOut : notnull => new(this, map);

    /// <inheritdoc />
    public virtual void ForEach(Action<TItem> action)
    {
        Option<TItem> next = Next();
        while (next.IsSome)
        {
            next.Inspect(action);
            next = Next();
        }
    }

    /// <inheritdoc />
    public virtual FilterAdapter<TItem> Filter(Func<TItem, bool> filter) =>
        new(this, filter);

    /// <inheritdoc />
    public virtual FilterMapAdapter<TItem, TOut> FilterMap<TOut>(
        Func<TItem, Option<TOut>> filterMap) where TOut : notnull =>
        new(this, filterMap);

    /// <inheritdoc />
    public virtual EnumerateAdapter<TItem> Enumerate() =>
        new(this);

    /// <inheritdoc />
    public virtual PeekableAdapter<TItem> Peekable() =>
        new(this);

    /// <inheritdoc />
    public virtual SkipWhileAdapter<TItem> SkipWhile(
        Func<TItem, bool> predicate) =>
        new(this, predicate);

    /// <inheritdoc />
    public virtual TakeWhileAdapter<TItem> TakeWhile(
        Func<TItem, bool> predicate) =>
        new(this, predicate);

    /// <inheritdoc />
    public virtual MapWhileAdapter<TItem, TOut> MapWhile<TOut>(
        Func<TItem, Option<TOut>> map) where TOut : notnull =>
        new(this, map);

    /// <inheritdoc />
    public SkipAdapter<TItem> Skip(int n) => new(this, n);

    /// <inheritdoc />
    public TakeAdapter<TItem> Take(int n) => new(this, n);

    /// <inheritdoc />
    public ScanAdapter<TItem, TState, TOut> Scan<TState, TOut>(
        TState initialState,
        Func<TState, TItem, Option<TOut>> scan)
        where TState : notnull where TOut : notnull =>
        new(this, initialState, scan);

    /// <inheritdoc />
    public virtual IEnumerator<Option<TItem>> GetEnumerator() =>
        this;

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public virtual IIterator<TItem> IntoIter() =>
        this;

    /// <summary>Releases the resources used by the iterator.</summary>
    /// <param name="disposing">
    /// A boolean value indicating whether to release both
    /// managed and unmanaged resources (true), or only unmanaged resources (false).
    /// </param>
    protected abstract void Dispose(bool disposing);
}
