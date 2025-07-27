namespace Waystone.Monads.Iterators.Extensions;

using System.Collections.Generic;
using Abstractions;
using Options;

/// <summary>
/// Provides extension methods for working with instances of
/// <see cref="IIterator{TItem}" />.
/// </summary>
public static class IteratorExtensions
{
    /// <summary>
    /// Collects all items from the given <see cref="IIterator{TItem}" /> and
    /// returns an <see cref="IEnumerable{T}" /> of the items.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of elements to be collected. Must be
    /// non-nullable.
    /// </typeparam>
    /// <param name="iterator">
    /// The <see cref="IIterator{TItem}" /> instance from which
    /// items are collected.
    /// </param>
    /// <returns>
    /// An <see cref="IEnumerable{T}" /> containing all items from the
    /// <paramref name="iterator" />.
    /// </returns>
    public static IEnumerable<TItem> Collect<TItem>(
        this Iterator<TItem> iterator) where TItem : notnull
    {
        using (iterator)
        {
            for (Option<TItem> next = iterator.Next();
                 next.IsSome;
                 next = iterator.Next())
            {
                yield return next.Unwrap();
            }
        }
    }
}
