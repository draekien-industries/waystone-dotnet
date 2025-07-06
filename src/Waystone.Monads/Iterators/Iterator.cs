namespace Waystone.Monads.Iterators;

using System.Collections;
using System.Collections.Generic;
using Waystone.Monads.Options;

/// <summary>
/// An iterator that returns an <see cref="Option{T}"/> when
/// enumerated.
/// </summary>
/// <typeparam name="T">The type of the value contained in the collection.</typeparam>
public abstract class Iterator<T> : IEnumerator<Option<T>>, IEnumerable<Option<T>>
    where T : notnull
{
    /// <summary>
    /// The current element in the iteration.
    /// </summary>
    protected Option<T> CurrentItem;

    /// <summary>
    /// The current index being accessed.
    /// </summary>
    protected int CurrentIndex;

    /// <summary>
    /// Creates a new instance of <see cref="Iterator{T}"/>
    /// </summary>
    protected Iterator()
    {
        CurrentItem = Option.None<T>();
        CurrentIndex = -1;
    }

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
}