namespace Waystone.Monads.Iterators;

using System.Collections.Generic;

/// <summary>
/// Provides functionality to convert an <see cref="IEnumerable{T}" />
/// into an <see cref="Iterator{TItem}" /> that enables sequential access to
/// elements.
/// </summary>
public static class IntoIterator
{
    /// <summary>
    /// Converts the specified <see cref="IEnumerable{T}" /> into an
    /// <see cref="Iterator{TItem}" /> to enable sequential access to its elements.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of elements in the source collection. Must be
    /// non-nullable.
    /// </typeparam>
    /// <param name="source">
    /// The source collection to convert into an
    /// <see cref="Iterator{TItem}" />.
    /// </param>
    /// <returns>
    /// An <see cref="Iterator{TItem}" /> providing sequential access to the
    /// elements of the source collection.
    /// </returns>
    public static Iterator<TItem> IntoIter<TItem>(
        this IEnumerable<TItem> source) where TItem : notnull => new(source);
}
