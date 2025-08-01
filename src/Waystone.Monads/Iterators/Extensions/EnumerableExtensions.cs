namespace Waystone.Monads.Iterators.Extensions;

using System.Collections.Generic;

/// <summary>
/// Extension methods for converting an <see cref="IEnumerable{T}" /> to
/// an <see cref="Iter{T}" />.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an <see cref="IEnumerable{T}" /> to an <see cref="Iter{T}" />
    /// .
    /// </summary>
    /// <param name="items">The <see cref="IEnumerable{T}" /> to convert.</param>
    /// <typeparam name="T">
    /// The type of items in the <see cref="IEnumerable{T}" />.
    /// Must be a non-nullable type.
    /// </typeparam>
    /// <returns>An <see cref="Iter{T}" /> containing the items from the</returns>
    public static Iter<T> IntoIter<T>(this IEnumerable<T> items)
        where T : notnull => new(items);
}
