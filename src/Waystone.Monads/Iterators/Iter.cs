namespace Waystone.Monads.Iterators;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Adapters;
using Extensions;
using Options;

/// <summary>
/// Represents a sequence of elements of type <typeparamref name="T" />
/// where each element is wrapped in an <see cref="Option{T}" />.
/// </summary>
/// <typeparam name="T">
/// The type of elements in the sequence. Must be a
/// non-nullable type.
/// </typeparam>
public class Iter<T> : IEnumerable<Option<T>> where T : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Iter{T}" /> class with
    /// the specified elements.
    /// </summary>
    /// <param name="elements">The sequence of elements to be managed by this iterator.</param>
    public Iter(IEnumerable<T> elements)
    {
        Elements = elements;
    }

    internal Iter(Iter<T> iter)
    {
        Elements = iter.Elements;
    }

    /// <summary>Represents a sequence of elements of type <typeparamref name="T" />.</summary>
    protected internal IEnumerable<T> Elements { get; }

    /// <summary>Gets the number of elements in the sequence.</summary>
    public Lazy<int> Count => new(() => Elements.Count());

    /// <summary>The current element in the sequence.</summary>
    protected internal Option<T> Current { get; private set; } =
        Option.None<T>();

    /// <summary>The index of the current element in the sequence.</summary>
    private int Index { get; set; } = -1;

    /// <inheritdoc />
    public IEnumerator<Option<T>> GetEnumerator()
    {
        for (Option<T> item = Next(); item.IsSome; item = Next())
        {
            yield return item;
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Advances the <see cref="Iter{T}" /> and returns the next value in the
    /// sequence.
    /// </summary>
    /// <returns>
    /// An <see cref="Option{T}" /> containing the next element in the
    /// sequence, or <see cref="Option.None{T}" /> when iteration is finished.
    /// Individual iterator implementations may choose to resume iteration, and so
    /// calling <see cref="Next" /> again may or may not eventually start returning
    /// <see cref="Some{T}" /> again at some point.
    /// </returns>
    public virtual Option<T> Next()
    {
        if (Index >= Count.Value)
        {
            return Option.None<T>();
        }

        Index++;
        T? element = Elements.ElementAtOrDefault(Index);
        return Current = element is null
            ? Option.None<T>()
            : Option.Some(element);
    }

    /// <summary>
    /// Returns the bounds on the remaining length of the
    /// <see cref="Iter{T}" />.
    /// </summary>
    /// <returns>
    /// A tuple where the first element is the lower bound, and the second
    /// element is the upper bound. <br /> The second half of the tuple is returned as
    /// an <see cref="Option{T}" />. A <see cref="None{T}" /> here means that either
    /// there is no known upper bound, or the upper bound is larger than
    /// <see cref="int" />.
    /// </returns>
    public virtual (int Lower, Option<int> Upper) SizeHint()
    {
        int count = Count.Value;

        return count switch
        {
            0 => (0, Option.None<int>()),
            var _ when Index == -1 => (count, Option.Some(count)),
            var _ when Index >= count => (0, Option.None<int>()),
            var _ => (count - Index, Option.Some(count - Index)),
        };
    }

    /// <summary>Takes two iterators and creates a new iterator over both sequences.</summary>
    /// <remarks>
    /// This method will return a new <see cref="Iter{T}" /> which will first
    /// iterate over values from the first iterator and then over values from the
    /// second iterator.
    /// </remarks>
    /// <param name="other">The second sequence to chain with the first one.</param>
    /// <returns>
    /// A new <see cref="Chain{T}" /> that contains the elements of both
    /// sequences.
    /// </returns>
    public Chain<T> Chain(IEnumerable<T> other) => new(this, other.IntoIter());

    /// <inheritdoc cref="Chain(System.Collections.Generic.IEnumerable{T})" />
    public Chain<T> Chain(Iter<T> other) => new(this, other);
}
