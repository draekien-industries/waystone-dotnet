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
    /// Represents an iterator that provides sequential access to elements of
    /// a specified type wrapped in <see cref="Option{T}" />.
    /// </summary>
    protected Iterator(Iterator<TItem> source) : this(source.Source)
    {
        Disposed = source.Disposed;
        NextCounter = source.NextCounter;
        Current = source.Current;
    }

    /// <summary>
    /// The source sequence of elements wrapped in <see cref="Option{T}" />.
    /// This represents the input collection from which the iterator retrieves items.
    /// </summary>
    protected internal IEnumerable<TItem> Source { get; set; }

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
        Dispose(true);
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
        if (!SourceEnumerator.IsValueCreated) return;

        SourceEnumerator.Value.Reset();
        NextCounter = 0;
        Current = Option.None<TItem>();
    }

    /// <inheritdoc />
    public Option<TItem> Current { get; protected set; }

    /// <inheritdoc />
    object? IEnumerator.Current => Current;

    /// <summary>
    /// Releases the unmanaged resources used by the
    /// <see cref="Iterator{TItem}" /> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and
    /// unmanaged resources; <see langword="false" /> to release only unmanaged
    /// resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed || !disposing) return;

        if (SourceEnumerator.IsValueCreated)
        {
            SourceEnumerator.Value.Dispose();
        }

        Disposed = true;
        NextCounter = 0;
        Current = Option.None<TItem>();
    }

    /// <summary>Garbage collector</summary>
    ~Iterator()
    {
        Dispose(false);
    }

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
        TItem current = SourceEnumerator.Value.Current;

        return current.Equals(default(TItem))
            ? Option.None<TItem>()
            : Option.Some(current);
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
    /// <see cref="IEnumerable{T}" />, creating a new
    /// <see cref="ChainIterator{TItem}" />.
    /// </summary>
    /// <param name="other">
    /// The second <see cref="IEnumerable{T}" /> to combine with
    /// the source.
    /// </param>
    /// <returns>
    /// A <see cref="ChainIterator{TItem}" /> representing the combined
    /// sequence.
    /// </returns>
    public ChainIterator<TItem> Chain(IEnumerable<TItem> other) =>
        new(this, other);

    /// <summary>Counts the number of elements in the iterator.</summary>
    /// <returns>The count of elements in the iterator.</returns>
    public int Count()
    {
        var count = 0;
        if (Disposed) return count;

        for (Option<TItem> next = Next(); next.IsSome; next = Next())
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Creates a new <see cref="EnumerateIterator{TItem}" /> that enumerates
    /// the elements of the current iterator, pairing each element with its
    /// corresponding index.
    /// </summary>
    /// <returns>
    /// An <see cref="EnumerateIterator{TItem}" /> that enumerates the
    /// elements with their indices.
    /// </returns>
    public EnumerateIterator<TItem> Enumerate() => new(this);

    /// <summary>
    /// Determines whether the elements of the current
    /// <see cref="Iterator{TItem}" /> are equal to the elements of another specified
    /// <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="other">
    /// An <see cref="IEnumerable{T}" /> to compare with the
    /// current iterator.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the two sequences are equal; otherwise,
    /// <see langword="false" />.
    /// </returns>
    public bool Eq(IEnumerable<TItem> other) => Source.SequenceEqual(other);

    /// <summary>
    /// Determines whether the elements of the current
    /// <see cref="Iterator{TItem}" /> are equal to the elements of another specified
    /// <see cref="Iterator{TItem}" />.
    /// </summary>
    /// <param name="other">
    /// An <see cref="Iterator{TItem}" /> to compare with the
    /// current iterator.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the two sequences are equal; otherwise,
    /// <see langword="false" />.
    /// </returns>
    public bool Eq(Iterator<TItem> other) => Eq(other.Source);

    /// <summary>
    /// Creates a new <see cref="FilterIterator{TItem}" /> that yields only elements
    /// matching the specified <paramref name="predicate" /> from the current iterator.
    /// </summary>
    /// <param name="predicate">
    /// A function to test each element for a condition. The function should returnW
    /// <see langword="true" /> for elements that should be included and <see langword="false" />
    /// for elements to be excluded.
    /// </param>
    /// <returns>
    /// A new <see cref="FilterIterator{TItem}" /> containing elements that satisfy the
    /// <paramref name="predicate" />.
    /// </returns>
    public FilterIterator<TItem> Filter(Func<TItem, bool> predicate) =>
        new(this, predicate);

    /// <summary>
    /// Creates a new instance of <see cref="MapIterator{TItem,TOut}" /> that
    /// applies a transformation function to each element of the source sequence.
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
    /// A new <see cref="MapIterator{TItem,TOut}" /> instance that provides
    /// elements of type <typeparamref name="TOut" /> transformed using the specified
    /// <paramref name="mapper" />.
    /// </returns>
    public MapIterator<TItem, TOut> Map<TOut>(Func<TItem, TOut> mapper)
        where TOut : notnull =>
        new(this, mapper);

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
