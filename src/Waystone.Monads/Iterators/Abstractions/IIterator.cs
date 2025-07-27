namespace Waystone.Monads.Iterators.Abstractions;

using System;
using System.Collections.Generic;
using Options;

/// <summary>
/// Provides a mechanism for sequential access to elements of a
/// collection, allowing iteration while leveraging optional values to signal
/// presence or absence of items.
/// </summary>
/// <typeparam name="TItem">
/// The type of elements iterated over. Must be
/// non-nullable.
/// </typeparam>
public interface IIterator<TItem> : IEnumerable<Option<TItem>>
    where TItem : notnull
{
    /// <summary>
    /// Retrieves the next <see cref="Option{T}" /> containing an item from
    /// the iterator, or a <see cref="None{T}" /> if the iterator is exhausted.
    /// </summary>
    /// <returns>
    /// An <see cref="Option{T}" /> containing the next item in the iterator
    /// or a <see cref="None{T}" /> if no items remain.
    /// </returns>
    Option<TItem> Next();

    /// <summary>
    /// Returns an estimate of the lower and upper bounds of the remaining
    /// items in the iterator.
    /// </summary>
    /// <returns>
    /// A tuple where the first element is the lower bound, and the second
    /// element is an <see cref="Option{T}" /> representing the upper bound or a
    /// <see cref="None{T}" /> if the upper bound is unknown.
    /// </returns>
    (int Lower, Option<int> Upper) SizeHint();

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
    bool All(Func<TItem, bool> predicate);
}
