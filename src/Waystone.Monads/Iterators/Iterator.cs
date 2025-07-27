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
public class Iterator<TItem> : IEnumerable<Option<TItem>>, IDisposable
    where TItem : notnull
{
    private readonly IEnumerable<TItem> _source;
    private readonly IEnumerator<TItem> _sourceEnumerator;
    private bool _disposed;

    /// <summary>Creates an instance of an <see cref="Iterator{TItem}" /></summary>
    /// <param name="source">The source <see cref="IEnumerable{T}" /></param>
    public Iterator(IEnumerable<TItem> source)
    {
        _source = source;
        _sourceEnumerator = _source.GetEnumerator();
        _disposed = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _sourceEnumerator.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public IEnumerator<Option<TItem>> GetEnumerator()
    {
        for (Option<TItem> next = Next(); next.IsSome; next = Next())
        {
            yield return next;
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
        if (_disposed || !_sourceEnumerator.MoveNext())
        {
            return Option.None<TItem>();
        }

        return Option.Some(_sourceEnumerator.Current);
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
        int size = _source.Count();
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
        if (_disposed) return true;
        if (SizeHint().Lower == 0) return true;

        bool next = Next().IsSomeAnd(predicate);

        while (next)
        {
            next = Next().IsSomeAnd(predicate);
            if (!next) return false;
        }

        return next;
    }

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
        if (_disposed) yield break;

        using (this)
        {
            for (Option<TItem> next = Next();
                 next.IsSome;
                 next = Next())
            {
                yield return next.Unwrap();
            }
        }
    }
}
