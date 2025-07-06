namespace Waystone.Monads.Iterators.Abstractions;

using System;
using System.Collections;
using System.Collections.Generic;
using Waystone.Monads.Iterators;
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

    /// <summary>
    /// Advances the iterator and returns the next value.
    /// </summary>
    /// <remarks>
    /// Individual iterator implementations may choose to resume iteration,
    /// and so calling `Next` again may or may not start returning <see cref="Some{T}"/>
    /// again at some point.
    /// </remarks>
    /// <returns>
    /// Returns <see cref="Some{T}"/> as long as there are elements.
    /// Returns <see cref="None{T}"/> when iteration is finished.
    /// </returns>
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

    /// <summary>
    /// Provides a size hint for the iterator. It can be used for
    /// optimisations such as reserving space for the elements
    /// of the iterator, but must not be trusted.
    /// </summary>
    /// <remarks>
    /// A <see cref="None{T}"/> in the upper bound means there is either
    /// no upper bound, or the upper bound is larger than `int.MaxValue`.
    /// </remarks>
    /// <returns>
    /// The bounds on the remaining length of the iterator.
    /// </returns>
    public virtual (int LowerBound, Option<int> UpperBound) SizeHint()
    {
        // Default implementation assumes an unbounded iterator.
        // Subclasses can override this to provide a more accurate size hint.
        return (0, Option.None<int>());
    }

    /// <summary>
    /// Returns the 'nth' element in the iterator. Note that calling 'Nth(0)' multiple times
    /// on the same iterator will return different elements.
    /// </summary>
    /// <remarks>
    /// Like most indexing operations, the count starts from zero,
    /// so `Nth(0)` returns the first element, `Nth(1)` the second, etc.
    /// </remarks>
    /// <param name="n"></param>
    /// <returns>
    /// Returns the 'nth' element in the iterator.
    /// </returns>
    public virtual Option<T> Nth(uint n)
    {
        CurrentIndex += (int)n;
        return Next();
    }

    /// <summary>
    /// Creates an <see cref="IIterator{T}"/> starting at the same point,
    /// but stepping by the given amount each iteration.
    /// </summary>
    /// <remarks>
    /// The first element of the iterator will always be returned, regardless
    /// of the step given.
    /// </remarks>
    /// <param name="interval">The interval to step by for each iteration</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The given interval cannot be 0.
    /// </exception>
    /// <returns>
    /// A <see cref="StepByIterator{T}"/> iterator.
    /// </returns>
    public virtual StepByIterator<T> StepBy(uint interval) => new(this, interval);
}
