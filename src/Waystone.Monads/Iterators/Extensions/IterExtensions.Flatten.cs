namespace Waystone.Monads.Iterators.Extensions;

using System.Collections.Generic;
using Adapters;

public static partial class IterExtensions
{
    /// <summary>
    /// Creates an <see cref="Iter{T}" /> that flattens a sequence of nested
    /// sequences into a single sequence of items.
    /// </summary>
    /// <param name="iter">
    /// The <see cref="Iter{TNested}" /> to flatten. The elements
    /// must be of type <see cref="IEnumerable{TItem}" />.
    /// </param>
    /// <typeparam name="T">
    /// The type of items in the nested sequences. Must be a
    /// non-nullable type.
    /// </typeparam>
    /// <returns>
    /// An iterator that flattens the sequence of nested sequences into a
    /// single sequence of items.
    /// </returns>
    public static Flatten<T> Flatten<T>(
        this Iter<T[]> iter)
        where T : notnull =>
        new(iter.Map(x => x.IntoIter()));

    /// <inheritdoc cref="Flatten{T}(Waystone.Monads.Iterators.Iter{T[]})" />
    public static Flatten<T> Flatten<T>(
        this Iter<IEnumerable<T>> iter)
        where T : notnull =>
        new(iter.Map(x => x.IntoIter()));

    /// <inheritdoc cref="Flatten{T}(Waystone.Monads.Iterators.Iter{T[]})" />
    public static Flatten<T> Flatten<T>(
        this Iter<List<T>> iter)
        where T : notnull =>
        new(iter.Map(x => x.IntoIter()));

    /// <inheritdoc cref="Flatten{T}(Waystone.Monads.Iterators.Iter{T[]})" />
    public static Flatten<T> Flatten<T>(
        this Iter<HashSet<T>> iter)
        where T : notnull =>
        new(iter.Map(x => x.IntoIter()));

    /// <inheritdoc cref="Flatten{T}(Waystone.Monads.Iterators.Iter{T[]})" />
    public static Flatten<T> Flatten<T>(
        this Iter<LinkedList<T>> iter)
        where T : notnull =>
        new(iter.Map(x => x.IntoIter()));

    /// <inheritdoc cref="Flatten{T}(Waystone.Monads.Iterators.Iter{T[]})" />
    public static Flatten<T> Flatten<T>(
        this Iter<Queue<T>> iter)
        where T : notnull =>
        new(iter.Map(x => x.IntoIter()));

    /// <inheritdoc cref="Flatten{T}(Waystone.Monads.Iterators.Iter{T[]})" />
    public static Flatten<T> Flatten<T>(
        this Iter<SortedSet<T>> iter)
        where T : notnull =>
        new(iter.Map(x => x.IntoIter()));

    /// <inheritdoc cref="Flatten{T}(Waystone.Monads.Iterators.Iter{T[]})" />
    public static Flatten<T> Flatten<T>(
        this Iter<Stack<T>> iter)
        where T : notnull =>
        new(iter.Map(x => x.IntoIter()));
}
