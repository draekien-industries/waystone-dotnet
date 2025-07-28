namespace Waystone.Monads.Iterators;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Options;

/// <summary>
/// Represents an iterator that provides sequential access to elements of
/// a specified type wrapped in <see cref="Option{T}" />.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements in the iterator. Must be a
/// non-nullable type.
/// </typeparam>
public class Iterator<TItem>
    : IEnumerable<Option<TItem>>, IEnumerator<Option<TItem>>
    where TItem : notnull
{
    /// <summary>Creates an instance of an <see cref="Iterator{TItem}" /></summary>
    /// <param name="source">The source <see cref="IEnumerable{T}" /></param>
    public Iterator(IEnumerable<TItem> source)
    {
        Source = source;
        SourceEnumerator =
            new Lazy<IEnumerator<TItem>>(() => Source.GetEnumerator());
        Disposed = false;
        NextCounter = 0;
        Current = Option.None<TItem>();
    }

    /// <summary>
    /// The source sequence of elements wrapped in <see cref="Option{T}" />.
    /// This represents the input collection from which the iterator retrieves items.
    /// </summary>
    protected IEnumerable<TItem> Source { get; }

    /// <summary>
    /// The enumerator for the source sequence of elements wrapped in
    /// <see cref="Option{T}" />. Provides the mechanism for iterating through the
    /// elements in the source collection.
    /// </summary>
    protected Lazy<IEnumerator<TItem>> SourceEnumerator { get; }

    /// <summary>
    /// Indicates whether the <see cref="Iterator{TItem}" /> instance has been
    /// disposed. When set to <see langword="true" />, it signifies that the
    /// <see cref="Iterator{TItem}" /> has released its resources and should not be
    /// used further.
    /// </summary>
    protected bool Disposed { get; set; }

    /// <summary>
    /// Tracks the number of items accessed through the iterator. This counter
    /// is incremented each time the <see cref="Iterator{TItem}.Next" /> method is
    /// called to retrieve the next item from the sequence.
    /// </summary>
    protected int NextCounter { get; set; }

    /// <inheritdoc />
    public IEnumerator<Option<TItem>> GetEnumerator() => this;

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void Dispose()
    {
        if (SourceEnumerator.IsValueCreated)
        {
            SourceEnumerator.Value.Dispose();
        }

        Disposed = true;
        NextCounter = 0;
        Current = Option.None<TItem>();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public bool MoveNext()
    {
        Current = Next();

        return Current.IsSome;
    }

    /// <inheritdoc />
    public void Reset()
    {
        if (Disposed) return;

        SourceEnumerator.Value.Reset();
        NextCounter = 0;
        Current = Option.None<TItem>();
    }

    /// <inheritdoc />
    public Option<TItem> Current { get; private set; }

    /// <inheritdoc />
    object? IEnumerator.Current => Current;

    /// <summary>
    /// Retrieves the next <see cref="Option{T}" /> containing an item from
    /// the iterator, or a <see cref="None{T}" /> if the iterator is exhausted.
    /// </summary>
    /// <returns>
    /// An <see cref="Option{T}" /> containing the next item in the iterator
    /// or a <see cref="None{T}" /> if no items remain.
    /// </returns>
    public virtual Option<TItem> Next()
    {
        if (Disposed || !SourceEnumerator.Value.MoveNext())
        {
            return Option.None<TItem>();
        }

        NextCounter++;
        return Option.Some(SourceEnumerator.Value.Current);
    }

    /// <summary>
    /// Returns an estimate of the lower and upper bounds of the remaining
    /// items in the iterator.
    /// </summary>
    /// <returns>
    /// A tuple where the first element is the lower bound, and the second
    /// element is an <see cref="Option{T}" /> representing the upper bound or a
    /// <see cref="None{T}" /> if the upper bound is unknown.
    /// </returns>
    public virtual (int Lower, Option<int> Upper) SizeHint()
    {
        if (Disposed) return (0, Option.None<int>());
        int size = Source.Count() - NextCounter;
        return size > 0 ? (size, Option.Some(size)) : (0, Option.None<int>());
    }

    /// <summary>
    /// Determines whether all elements in the iterator satisfy a specified
    /// condition.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// <see langword="true" /> if the iterator contains no elements or all
    /// elements satisfy the condition specified by <paramref name="predicate" />;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public bool All(Func<TItem, bool> predicate)
    {
        if (Disposed) return true;
        if (SizeHint().Lower == 0) return true;

        for (Option<TItem> next = Next(); next.IsSome; next = Next())
        {
            if (next.Filter(predicate).IsNone) return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether any elements in the iterator satisfy the specified
    /// predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// <see langword="true" /> if any elements satisfy the predicate;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public bool Any(Func<TItem, bool> predicate)
    {
        if (Disposed) return false;
        if (SizeHint().Lower == 0) return false;

        for (Option<TItem> next = Next(); next.IsSome; next = Next())
        {
            if (next.Filter(predicate).IsSome) return true;
        }

        return false;
    }

    /// <summary>
    /// Combines the elements of the current iterator with those of another
    /// <see cref="IEnumerable{T}" />, creating a new <see cref="ChainIterator{TItem}" />.
    /// </summary>
    /// <param name="other">
    /// The second <see cref="IEnumerable{T}" /> to combine with
    /// the source.
    /// </param>
    /// <returns>A <see cref="ChainIterator{TItem}" /> representing the combined sequence.</returns>
    public ChainIterator<TItem> Chain(IEnumerable<TItem> other) => new(Source, other);

    /// <summary>
    /// Creates a new instance of <see cref="MapIterator{TItem,TOut}" /> that applies
    /// a transformation function to each element of the source sequence.
    /// </summary>
    /// <param name="mapper">
    /// A transformation function to apply to each element of type
    /// <typeparamref name="TItem" />.
    /// </param>
    /// <typeparam name="TOut">
    /// The type of the output values, which must be
    /// non-nullable.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="MapIterator{TItem,TOut}" /> instance that provides elements
    /// of type <typeparamref name="TOut" /> transformed using the specified
    /// <paramref name="mapper" />.
    /// </returns>
    public MapIterator<TItem, TOut> Map<TOut>(Func<TItem, TOut> mapper)
        where TOut : notnull =>
        new(Source, mapper);

    /// <summary>
    /// Collects all remaining elements from the iterator and returns them as
    /// an <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}" /> containing all elements in the
    /// iterator that have not yet been yielded.
    /// </returns>
    public IEnumerable<TItem> Collect()
    {
        if (Disposed) yield break;

        for (Option<TItem> next = Next();
             next.IsSome;
             next = Next())
        {
            yield return next.Unwrap();
        }
    }
}
