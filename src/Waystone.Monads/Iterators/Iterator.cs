namespace Waystone.Monads.Iterators;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abstractions;
using Options;

/// <summary>
/// Represents an iterator that provides mechanisms for sequential access
/// to elements of a collection and integrates with the
/// <see cref="IIterator{TItem}" /> interface.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements iterated over. Must be
/// non-nullable.
/// </typeparam>
public struct Iterator<TItem> : IIterator<TItem>, IDisposable
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
    public IEnumerator<Option<TItem>> GetEnumerator()
    {
        for (Option<TItem> next = Next(); next.IsSome; next = Next())
        {
            yield return next;
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public Option<TItem> Next()
    {
        if (_disposed || !_sourceEnumerator.MoveNext())
        {
            return Option.None<TItem>();
        }

        return Option.Some(_sourceEnumerator.Current);
    }

    /// <inheritdoc />
    public (int Lower, Option<int> Upper) SizeHint()
    {
        int size = _source.Count();
        return size > 0 ? (size, Option.Some(size)) : (0, Option.None<int>());
    }

    /// <inheritdoc />
    public IEnumerable<TItem> Collect()
    {
        using (this)
        {
            if (_disposed) yield break;

            for (Option<TItem> next = Next(); next.IsSome; next = Next())
            {
                yield return next.Unwrap();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _sourceEnumerator.Dispose();
        _disposed = true;
    }
}
