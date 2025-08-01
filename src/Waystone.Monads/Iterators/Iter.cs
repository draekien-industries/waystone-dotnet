namespace Waystone.Monads.Iterators;

using System.Collections;
using System.Collections.Generic;
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

    /// <inheritdoc />
    public IEnumerator<Option<T>> GetEnumerator()
    {
        foreach (T item in Elements)
        {
            if (item.Equals(default(T)))
            {
                yield return Option.None<T>();
                continue;
            }

            yield return Option.Some(item);
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
