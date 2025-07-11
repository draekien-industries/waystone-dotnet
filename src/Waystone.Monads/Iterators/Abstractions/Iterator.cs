namespace Waystone.Monads.Iterators.Abstractions;

using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Options;
using Primitives;

/// <summary>
/// An enumerable that returns an <see cref="Option{T}" /> when
/// enumerated.
/// </summary>
/// <typeparam name="T">The type of the value contained in the iterator.</typeparam>
public abstract class Iterator<T>
    : IEnumerator<Option<T>>, IEnumerable<Option<T>>
    where T : notnull
{
    /// <summary>Creates a new instance of <see cref="Iterator{T}" /></summary>
    protected Iterator()
    {
        CurrentItem = Option.None<T>();
        CurrentIndex = -1;
    }

    /// <summary>The current element in the iteration.</summary>
    protected internal Option<T> CurrentItem { get; set; }

    /// <summary>The current index being accessed.</summary>
    protected internal int CurrentIndex { get; set; }

    /// <inheritdoc />
    public IEnumerator<Option<T>> GetEnumerator() => this;

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public virtual Option<T> Current => CurrentItem;

    object IEnumerator.Current => Current;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public abstract bool MoveNext();

    /// <inheritdoc />
    public virtual void Reset()
    {
        CurrentIndex = -1;
        CurrentItem = Option.None<T>();
    }

    /// <summary>Advances the iterator and returns the next value.</summary>
    /// <remarks>
    /// Individual iterator implementations may choose to resume iteration,
    /// and so calling `Next` again may or may not start returning
    /// <see cref="Some{T}" /> again at some point.
    /// </remarks>
    /// <returns>
    /// Returns <see cref="Some{T}" /> as long as there are elements. Returns
    /// <see cref="None{T}" /> when iteration is finished.
    /// </returns>
    public virtual Option<T> Next() => MoveNext() ? Current : Option.None<T>();

    /// <summary>
    /// Releases the resources used by the <see cref="Iterator{T}" />
    /// instance.
    /// </summary>
    /// <param name="disposing">
    /// A boolean indicating whether the method is being called
    /// directly or by the garbage collector. If <c>true</c>, the method has been
    /// called directly to release managed and unmanaged resources. If <c>false</c>,
    /// the method has been called by the finalizer to release only unmanaged
    /// resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    { }

    ~Iterator()
    {
        Dispose(false);
    }

    /// <summary>
    /// Provides a size hint for the iterator. It can be used for
    /// optimisations such as reserving space for the elements of the iterator, but
    /// must not be trusted.
    /// </summary>
    /// <remarks>
    /// A <see cref="None{T}" /> in the upper bound means there is either no
    /// upper bound, or the upper bound is larger than `int.MaxValue`.
    /// </remarks>
    /// <returns>The bounds on the remaining length of the iterator.</returns>
    public virtual (PosInt LowerBound, Option<PosInt> UpperBound) SizeHint() =>
        // Default implementation assumes an unbounded iterator.
        // Subclasses can override this to provide a more accurate size hint.
        (0, Option.None<PosInt>());

    /// <summary>
    /// Returns the 'nth' element in the iterator. Note that calling 'Nth(0)'
    /// multiple times on the same iterator will return different elements.
    /// </summary>
    /// <remarks>
    /// Like most indexing operations, the count starts from zero, so `Nth(0)`
    /// returns the first element, `Nth(1)` the second, etc.
    /// </remarks>
    /// <param name="n"></param>
    /// <returns>Returns the 'nth' element in the iterator.</returns>
    public virtual Option<T> Nth(PosInt n)
    {
        for (var i = 0; i <= n; i++)
        {
            if (!MoveNext())
            {
                return Option.None<T>();
            }
        }

        return Current;
    }

    /// <summary>
    /// Creates an <see cref="Iterator{T}" /> starting at the same point, but
    /// stepping by the given amount each iteration.
    /// </summary>
    /// <remarks>
    /// The first element of the iterator will always be returned, regardless
    /// of the step given.
    /// </remarks>
    /// <param name="interval">The interval to step by for each iteration</param>
    /// <exception cref="ArgumentOutOfRangeException">The given interval cannot be 0.</exception>
    /// <returns>A <see cref="StepByIterator{T}" /> iterator.</returns>
    public virtual StepByIterator<T> StepBy(PosInt interval) =>
        new(this, interval);

    /// <summary>Takes two iterators and creates a new iterator over both in sequence.</summary>
    /// <param name="other">The second iterator to iterate over.</param>
    /// <returns>
    /// A new iterator which will first iterate over values from the first
    /// iterator, then over values from the second iterator.
    /// </returns>
    public ChainIterator<T> Chain(Iterator<T> other) => new(this, other);

    /// <summary>Takes two iterators and creates a new iterator over both in sequence.</summary>
    /// <param name="other">The second enumerable to iterate over.</param>
    /// <returns>
    /// A new iterator which will first iterate over values from the first
    /// iterator, then over values from the second iterator.
    /// </returns>
    public ChainIterator<T> Chain(IEnumerable<T> other) =>
        Chain(other.IntoIter());
}
