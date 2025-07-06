namespace Waystone.Monads.Iterators.Abstractions;

using System;
using System.Collections;
using System.Collections.Generic;
using Waystone.Monads.Iterators;
using Waystone.Monads.Iterators.Primitives;
using Waystone.Monads.Options;

/// <summary>
/// An <see cref="IIterator{T}"/> that returns an <see cref="Option{T}"/> when
/// enumerated.
/// </summary>
/// <typeparam name="T">The type of the value contained in the iterator.</typeparam>
public abstract class Iterator<T> : IIterator<T>
    where T : notnull
{
    /// <summary>
    /// Creates a new instance of <see cref="Iterator{T}"/>
    /// </summary>
    protected Iterator()
    {
        CurrentItem = Option.None<T>();
        CurrentIndex = -1;
    }

    /// <summary>
    /// The current element in the iteration.
    /// </summary>
    protected internal Option<T> CurrentItem { get; set; }

    /// <summary>
    /// The current index being accessed.
    /// </summary>
    protected internal int CurrentIndex { get; set; }

    /// <inheritdoc />
    public Option<T> Current => CurrentItem;

    object IEnumerator.Current => Current;

    /// <inheritdoc/>
    public virtual Option<T> Next()
    {
        return MoveNext() ? Current : Option.None<T>();
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        // no resource to dispose by default
    }

    /// <inheritdoc />
    public abstract bool MoveNext();

    /// <inheritdoc />
    public virtual void Reset()
    {
        CurrentIndex = -1;
        CurrentItem = Option.None<T>();
    }

    /// <inheritdoc />
    public IEnumerator<Option<T>> GetEnumerator() => this;

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public virtual (PosInt LowerBound, Option<PosInt> UpperBound) SizeHint()
    {
        // Default implementation assumes an unbounded iterator.
        // Subclasses can override this to provide a more accurate size hint.
        return (0, Option.None<PosInt>());
    }

    /// <inheritdoc/>
    public virtual Option<T> Nth(PosInt n)
    {
        CurrentIndex += n;
        return Next();
    }

    /// <inheritdoc/>
    public virtual StepByIterator<T> StepBy(PosInt interval) => new(this, interval);
}
